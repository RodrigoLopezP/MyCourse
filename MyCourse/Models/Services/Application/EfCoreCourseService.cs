using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyCourse.Models.Entities;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Application
{
     public class EfCoreCourseService //: ICourseService
     {
          private readonly MyCourseDbContext dbContext;
          private readonly IOptionsMonitor<CoursesOptions> _coursesOpts;
          public EfCoreCourseService(MyCourseDbContext dbContext, IOptionsMonitor<CoursesOptions> coursesOptions)
          {
               this._coursesOpts = coursesOptions;
               this.dbContext = dbContext;
          }

          public async Task<CourseDetailViewModel> GetCourseAsync(int id)
          {
               CourseDetailViewModel courseDetail = await
               dbContext.Courses
               .Where(course => course.Id == id)
               .AsNoTracking()//EF no farà il log tracking, utile per aumentare le prestazione. Usare solo se facciamo delle SELECT
               .Select(course => new CourseDetailViewModel
               {
                    Id = course.Id,
                    Title = course.Title,
                    ImagePath = course.ImagePath,
                    Author = course.Author,
                    Rating = course.Rating,
                    CurrentPrice = course.CurrentPrice,
                    FullPrice = course.FullPrice,

                    Description = course.Description,
                    Lessons = course.Lessons.Select(lesson => new LessonViewModel
                    {
                         Id = lesson.Id,
                         Title = lesson.Title,
                         Description = lesson.Description,
                         Duration = lesson.Duration,
                    }).ToList(), //qui non server ASYNC perché interagiamo effettivamente con il db con .SINGLEASYNC()
               })
               .SingleAsync(); // restituisce 1 elem, se ci sono 0 o più di uno = ECCEZIONE
                               //.firstAsync();// restituisce primo elem, se ci sono più di uno OK, raccatta comunque il primo
                               //.singleODefaultAsync(); //Come singleAsync, ma se è VUOTO va bene, dà NULL
                               //.FirstOrDefaultAxync() // se è 1 solo ok, se è vuoto ok, se è più di 1 raccatta il primo. E' IL PIU' TOLLERANTE

               return courseDetail;
          }

          public async Task<List<CourseViewModel>> GetCoursesAsync(string search, int page, string orderBy, bool ascending)
          {
               search = search ?? ""; // NULL COALESCINE OPERATOR -  se vale null assegna un valore di default .... Sez 13 - 88 - Implementare la funzionalità di ricerca
               int limit = _coursesOpts.CurrentValue.PerPage;
               int offset = (page - 1) * limit;
               #region IQueryable di base, per applicare i primi filtri se presenti
               var orderOptions = _coursesOpts.CurrentValue.Order;
               if (!orderOptions.Allow.Contains(orderBy))
               {
                    orderBy = orderOptions.By;
                    ascending = orderOptions.Ascending;
               }
               IQueryable<Course> baseQuery = dbContext.Courses;
               switch (orderBy)
               {
                    case "Title":
                         if (ascending)
                         {
                              baseQuery = baseQuery.OrderBy(course => course.Title);
                         }
                         else
                         {
                              baseQuery = baseQuery.OrderByDescending(course => course.Title);
                         }
                         break;
                    case "Rating":
                         if (ascending)
                         {
                              baseQuery = baseQuery.OrderBy(course => course.Rating);
                         }
                         else
                         {
                              baseQuery = baseQuery.OrderByDescending(course => course.Rating);
                         }
                         break;
                    case "CurrentPrice":
                         if (ascending)
                         {
                              baseQuery = baseQuery.OrderBy(course => course.CurrentPrice.Amount);
                         }
                         else
                         {
                              baseQuery = baseQuery.OrderByDescending(course => course.CurrentPrice.Amount);
                         }
                         break;
               }
               #endregion
               #region IQueryable finale, dove aggiungiamo i filtri non dinamici, e le colonne che vogliamo ottenere
               IQueryable<CourseViewModel> queryEF = baseQuery
               .AsNoTracking()//EF no farà il log tracking, utile per aumentare le prestazione. Usare solo se facciamo delle SELECT
               .Where(x => x.Title.Contains(search))
               .Select(course => new CourseViewModel
               {
                    Id = course.Id,
                    Title = course.Title,
                    ImagePath = course.ImagePath,
                    Author = course.Author,
                    Rating = course.Rating,
                    CurrentPrice = course.CurrentPrice,
                    FullPrice = course.FullPrice,
               })
               .Skip(offset)
               .Take(limit);
               #endregion
               //Con ToList eseguiamo effettivamente la query sul db per ottenere il risultato
               List<CourseViewModel> courses = await queryEF.ToListAsync();
               return courses;
          }
     }
}