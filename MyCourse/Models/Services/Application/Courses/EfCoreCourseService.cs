using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCourse.Models.Exceptions;
using MyCourse.Models.InputModels;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels;
using MyCourse.Models.Entities;
using MyCourse.Models.Services.Application.Courses;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.Enums;
using Microsoft.AspNetCore.Http;
using MyCourse.Models.Exceptions.Application;
using System.Security.Claims;

namespace MyCourse.Models.Services.Application.Courses
{
     public class EfCoreCourseService : ICourseService
     {
          private readonly ILogger<EfCoreCourseService> _logger;
          private readonly IImagePersister imagePersister;
          private readonly MyCourseDbContext dbContext;
          private readonly IOptionsMonitor<CoursesOptions> _coursesOpts;
          private readonly IHttpContextAccessor _httpContextAccessor;
          public EfCoreCourseService(IHttpContextAccessor httpContextAccessor, MyCourseDbContext dbContext, IOptionsMonitor<CoursesOptions> coursesOptions, ILogger<EfCoreCourseService> logger, IImagePersister imagePersister)
          {
               _httpContextAccessor = httpContextAccessor;
               _coursesOpts = coursesOptions;
               this.dbContext = dbContext;
               _logger = logger;

               this.imagePersister = imagePersister;

          }

          public async Task<CourseDetailViewModel> GetCourseAsync(int id)
          {
               IQueryable<CourseDetailViewModel> queryLinq = dbContext.Courses
               .Include(course => course.Lessons) //vengono aggiunti anche le lezioni di ogni corso
               .Where(course => course.Id == id)
               .AsNoTracking()                                         //EF no farà il log tracking, utile per aumentare le prestazione. Usare solo se facciamo delle SELECT
               .Select(course => CourseDetailViewModel.FromEntity(course)); //qui non server ASYNC perché interagiamo effettivamente con il db con .SINGLEASYNC()
               { }
               CourseDetailViewModel dettaglioCorso = await queryLinq.SingleAsync();
               // restituisce 1 elem, se ci sono 0 o più di uno = ECCEZIONE
               //.firstAsync();// restituisce primo elem, se ci sono più di uno OK, raccatta comunque il primo
               //.singleODefaultAsync(); //Come singleAsync, ma se è VUOTO va bene, dà NULL
               //.FirstOrDefaultAxync() // se è 1 solo ok, se è vuoto ok, se è più di 1 raccatta il primo. E' IL PIU' TOLLERANTE

               if (dettaglioCorso == null)
               {
                    _logger.LogWarning("Course {id} not found", id);
                    throw new CourseNotFoundException(id);
               }


               return dettaglioCorso;
          }

          public async Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel coursesFilters)
          {
               #region IQueryable di base, per applicare i primi filtri se presenti
               IQueryable<Course> baseQuery = dbContext.Courses;
               //Grazie a C# 8.0, viene usata questa switch expression che sostuituisce il switch case che c'era prima ed era brutto 
               baseQuery = (coursesFilters.OrderBy, coursesFilters.Ascending) switch
               {
                    ("Title", true) => baseQuery.OrderBy(x => x.Title),
                    ("Title", false) => baseQuery.OrderByDescending(x => x.Title),

                    ("Rating", true) => baseQuery.OrderBy(x => x.Rating),
                    ("Rating", false) => baseQuery.OrderByDescending(x => x.Rating),

                    ("CurrentPrice", true) => baseQuery.OrderBy(x => x.CurrentPrice.Amount),
                    ("CurrentPrice", false) => baseQuery.OrderByDescending(x => x.CurrentPrice.Amount),

                    ("Id", true) => baseQuery.OrderBy(x => x.Id),
                    ("Id", false) => baseQuery.OrderByDescending(x => x.Id),

                    _ => baseQuery,
               };
               #endregion

               #region IQueryable con la quale prendiamo tutti i risultati della intera tabella
               IQueryable<Course> queryEF = baseQuery
               .AsNoTracking()//EF no farà il log tracking, utile per aumentare le prestazione. Usare solo se facciamo delle SELECT
               .Where(x => x.Title.ToLower().Contains(coursesFilters.Search.ToLower()));

               #endregion
               //Con ToList eseguiamo effettivamente la query sul db per ottenere il risultato
               List<CourseViewModel> courses = await queryEF
                       .Skip(coursesFilters.Offset)
                       .Take(coursesFilters.Limit)
                                           .Select(course => CourseViewModel.FromEntity(course))
                                           .ToListAsync(); //Skup e TAKE li portiamo qui perche queryEF ci servirà intera sotto per il count

               int totCourses = await queryEF.CountAsync();

               ListViewModel<CourseViewModel> result = new()
               {
                    Results = courses,
                    TotalCount = totCourses
               };

               return result;
          }

          public async Task<List<CourseViewModel>> GetMostRecentCoursesAsync()
          {
               CourseListInputModel inputForMostRecentCourses = new(
                   search: "",
                   page: 1,
                   orderBy: "Id",
                   ascending: false,
                   limit: _coursesOpts.CurrentValue.inHome,
                   coursesOptions: _coursesOpts.CurrentValue.Order);
               ListViewModel<CourseViewModel> coursesList_ViewModel = await GetCoursesAsync(inputForMostRecentCourses);
               return coursesList_ViewModel.Results;
          }
          public async Task<List<CourseViewModel>> GetBestRatingCoursesAsync()
          {
               CourseListInputModel inputForBestRatingCourses = new(
                   search: "",
                   page: 1,
                   orderBy: "Rating",
                   ascending: false,
                   limit: _coursesOpts.CurrentValue.inHome,
                   coursesOptions: _coursesOpts.CurrentValue.Order);
               ListViewModel<CourseViewModel> coursesList_ViewModel = await GetCoursesAsync(inputForBestRatingCourses);
               return coursesList_ViewModel.Results;
          }

          public async Task<CourseDetailViewModel> CreateCourseAsync(CourseCreateInputModel nuovoCorso)
          {
               string title = nuovoCorso.Title;
               string author;
               string authorId;
               try
               {
                    author = _httpContextAccessor.HttpContext.User.FindFirst("FullName").Value;
                    authorId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

               }
               catch (NullReferenceException)
               {
                    throw new UserUnknownException();
               }
               var courseEnt = new Course(title, author, authorId);
               dbContext.Add(courseEnt);
               try
               {
                    await dbContext.SaveChangesAsync();
               }
               catch (DbUpdateException exc) when (exc.InnerException is SqliteException { SqliteErrorCode: 19 })
               {
                    throw new CourseTitleUnavailableException(title, exc);
               }
               return CourseDetailViewModel.FromEntity(courseEnt);
          }

          public async Task<bool> IsTitleAvailableAsync(string title, int id)
          {
               //await dbContext.Courses.AnyAsync(course => course.Title == title);
               bool titleExists = await dbContext.Courses.Where(x => x.Id != id).AnyAsync(course =>
               EF.Functions.Like(course.Title, title));
               return !titleExists;
          }

          public async Task<CourseEditInputModel> GetCourseForEditingAsync(int id)
          {
               IQueryable<CourseEditInputModel> queryLinq = dbContext.Courses
                    .Where(course => course.Id == id)
                    .AsNoTracking()                                         //EF no farà il log tracking, utile per aumentare le prestazione. Usare solo se facciamo delle SELECT
                    .Select(course => CourseEditInputModel.FromEntity(course));
               //alla fine SELECT il corso con i valori che ci interessano,
               //quindi usiamo la func FromEntity che abbiamo creato,
               //altrimenti viene un codice troppo complicato da vedere

               CourseEditInputModel dettaglioCorso = await queryLinq.SingleAsync();
               // restituisce 1 elem, se ci sono 0 o più di uno = ECCEZIONE
               if (dettaglioCorso == null)
               {
                    _logger.LogWarning("Course {id} not found", id);
                    throw new CourseNotFoundException(id);
               }
               return dettaglioCorso;
          }

          public async Task<CourseDetailViewModel> EditCourseAsync(CourseEditInputModel inputModel)
          {
               Course course = await dbContext.Courses.FindAsync(inputModel.Id);

               course.ChangeTitle(inputModel.Title);
               course.ChangeDescription(inputModel.Description);
               course.ChangePrice(inputModel.FullPrice, inputModel.CurrentPrice);
               course.ChangeEmail(inputModel.Email);
               course.ChangeStatus(CourseStatus.Published);
               dbContext.Entry(course).Property(course => course.RowVersion).OriginalValue = inputModel.RowVersion;

               //cambia img del corso se è presente una nuova
               if (inputModel.Image != null)
               {
                    try
                    {
                         string imagePath = await imagePersister.SaveCourseImageAsync(inputModel.Id, inputModel.Image);
                         course.ChangeImagePath(imagePath);
                    }
                    catch (Exception exc)
                    {
                         throw new CourseImageInvalidException(course.Id, exc);
                    }
               }

               //Salvataggio modifiche in DB
               try
               {
                    await dbContext.SaveChangesAsync();
               }
               catch (DbUpdateException exc) when (exc.InnerException is SqliteException { SqliteErrorCode: 19 })
               {
                    throw new CourseTitleUnavailableException(inputModel.Title, exc);
               }
               catch (DbUpdateConcurrencyException)
               {
                    throw new OptimisticConcurrencyException();
               }

               return CourseDetailViewModel.FromEntity(course);
          }

          public async Task DeleteCourseAsync(CourseDeleteInputModel inputModel)
          {
               Course course = await dbContext.Courses.FindAsync(inputModel.Id);

               if (course == null)
               {
                    throw new CourseNotFoundException(inputModel.Id);
               }

               course.ChangeStatus(CourseStatus.Deleted);
               await dbContext.SaveChangesAsync();
          }

          public Task SendQuestionToCourseAuthorAsync(int id, string question)
          {
               throw new NotImplementedException();
          }
          public Task<string> GetCourseAuthorIdAsync(int courseId)
          {
               return dbContext.Courses
                               .Where(course => course.Id == courseId)
                               .Select(course => course.AuthorId)
                               .FirstOrDefaultAsync();
          }
          public Task<int> GetCourseCountByAuthorIdAsync(string authorId)
          {
               return dbContext.Courses
                               .Where(course => course.AuthorId == authorId)
                               .CountAsync();
          }
     }
}