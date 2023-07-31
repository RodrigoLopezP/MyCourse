using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCourse.Models.Exceptions;
using MyCourse.Models.InputModels;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ValueObjects;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Application
{
     public class AdoNetCourseService : ICourseService
     {
          private readonly IDatabaseAccessor db;
          private readonly IImagePersister imagePersister;
          private readonly IOptionsMonitor<CoursesOptions> _coursesOpts;
          /*Lez-12-72 - IOptionsMonitor<CoursesOptions> coursesOptions 
          *-Leggere la configurazione del appsetting.json in modo tipizzato */
          /*Lez-12-72 - IOptionsMonitor<CoursesOptions> coursesOptions 
          *-Leggere la configurazione del appsetting.json in modo tipizzato */
          private readonly ILogger<AdoNetCourseService> _logger;
          public AdoNetCourseService(ILogger<AdoNetCourseService> logger, IDatabaseAccessor db, IImagePersister imagePersister, IOptionsMonitor<CoursesOptions> coursesOptions)
          {
               _logger = logger;
               this._coursesOpts = coursesOptions;
               this.db = db;

               this.imagePersister = imagePersister;// usiamo questo oggetto per usarlo nella metodo EditCourseAsync
          }
          public async Task<List<CourseViewModel>> GetBestRatingCoursesAsync()
          {
               CourseListInputModel inputForBestRatingCourses = new CourseListInputModel(
                  search: "",
                  page: 1,
                  orderBy: "Rating",
                  ascending: false,
                  limit: _coursesOpts.CurrentValue.inHome,
                  coursesOptions: _coursesOpts.CurrentValue.Order);
               ListViewModel<CourseViewModel> coursesList_ViewModel = await GetCoursesAsync(inputForBestRatingCourses);
               return coursesList_ViewModel.Results;
          }
          public async Task<List<CourseViewModel>> GetMostRecentCoursesAsync()
          {
               CourseListInputModel inputForMostRecentCourses = new CourseListInputModel(
                  search: "",
                  page: 1,
                  orderBy: "Id",
                  ascending: false,
                  limit: _coursesOpts.CurrentValue.inHome,
                  coursesOptions: _coursesOpts.CurrentValue.Order);
               ListViewModel<CourseViewModel> coursesList_ViewModel = await GetCoursesAsync(inputForMostRecentCourses);
               return coursesList_ViewModel.Results;
          }
          public async Task<CourseDetailViewModel> GetCourseAsync(int id)
          {
               _logger.LogInformation("Course {id} requested", id);

               FormattableString query = $@"SELECT Id, Title, Description, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency, RowVersion FROM Courses WHERE Id={id}
            ; SELECT Id, Title, Description, Duration FROM Lessons WHERE CourseId={id}";

               DataSet dataSet = await db.QueryAsync(query);

               //Course
               var courseTable = dataSet.Tables[0];
               if (courseTable.Rows.Count != 1)
               {
                    _logger.LogWarning("Course {id} not found", id);
                    throw new CourseNotFoundException(id);
               }
               DataRow courseRow = courseTable.Rows[0];
               var courseDetailViewModel = CourseDetailViewModel.FromDataRow(courseRow);

               //Course lessons
               var lessonDataTable = dataSet.Tables[1];

               foreach (DataRow lessonRow in lessonDataTable.Rows)
               {
                    LessonViewModel lessonViewModel = LessonViewModel.FromDataRow(lessonRow);
                    courseDetailViewModel.Lessons.Add(lessonViewModel);
               }
               return courseDetailViewModel;
          }
          public async Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel coursesFilters)
          {
               string orderby = coursesFilters.OrderBy == "CurrentPrice" ? "CurrentPrice_Amount" : coursesFilters.OrderBy;
               string direction = coursesFilters.Ascending ? "ASC" : "DESC";

               FormattableString query = $@"SELECT Id, Title, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Courses WHERE Title LIKE {"%" + coursesFilters.Search + "%"} ORDER BY {(Sql)orderby} {(Sql)direction} LIMIT {coursesFilters.Limit} OFFSET {coursesFilters.Offset}; 
            SELECT COUNT(*) FROM Courses WHERE Title LIKE {"%" + coursesFilters.Search + "%"}";
               DataSet dataSet = await db.QueryAsync(query);
               var dataTable = dataSet.Tables[0];
               var courseList = new List<CourseViewModel>();
               foreach (DataRow courseRow in dataTable.Rows)
               {
                    CourseViewModel courseViewModel = CourseViewModel.FromDataRow(courseRow);
                    courseList.Add(courseViewModel);
               }

               ListViewModel<CourseViewModel> result = new ListViewModel<CourseViewModel>
               {
                    Results = courseList,
                    TotalCount = Convert.ToInt32(dataSet.Tables[1].Rows[0][0])
               };

               return result;
          }
          public async Task<CourseDetailViewModel> CreateCourseAsync(CourseCreateInputModel nuovoCorso)
          {
               string title = nuovoCorso.Title;
               string author = "Mario Rossi";
               string defImgPath = "/Courses/default.png";
               try
               {
                    int courseId = await db.QueryScalarAsync<int>($@"INSERT INTO Courses (Title, Author, ImagePath, CurrentPrice_Currency, CurrentPrice_Amount, FullPrice_Currency, FullPrice_Amount) VALUES ({title}, {author}, {defImgPath}, 'EUR', 0, 'EUR', 0);
                                                 SELECT last_insert_rowid();");
                    CourseDetailViewModel course = await GetCourseAsync(courseId);
                    return course;
               }
               catch (SqliteException exc) when (exc.SqliteErrorCode == 19)
               {
                    //errore 19 è quando aviene una exception per una UNIQUE, in questo caso quella del title
                    throw new CourseTitleUnavailableException(title, exc);
               }
          }
          public async Task<bool> IsTitleAvailableAsync(string title, int id)
          {
               //tanto questo risultato viene sempre o 0 o 1, quindi possiamo convertirlo in bool direttamente
               bool titleExists = await db.QueryScalarAsync<bool>($"SELECT COUNT(*) FROM Courses WHERE Title LIKE {title} AND Id<>{id}");
               return !titleExists;
          }
          public async Task<CourseEditInputModel> GetCourseForEditingAsync(int id)
          {
               FormattableString query = $@"
               SELECT Id, Title, Description, ImagePath,Email , FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency, RowVersion
               FROM Courses
               WHERE Id LIKE {id}";
               DataSet dataSet = await db.QueryAsync(query);
               var dataTable = dataSet.Tables[0];

               if (dataTable.Rows.Count != 1)
               {
                    _logger.LogWarning("Course {id} non found", id);
                    throw new CourseNotFoundException(id);
               }
               DataRow rowCourseToEdit = dataTable.Rows[0];
               CourseEditInputModel courseToEdit = CourseEditInputModel.FromDataRow(rowCourseToEdit);
               return courseToEdit;
          }
          public async Task<CourseDetailViewModel> EditCourseAsync(CourseEditInputModel inputModel)
          {
               try
               {
                    string imagePath = null;
                    if (inputModel.Image != null)
                    {
                         imagePath = await imagePersister.SaveCourseImageAsync(inputModel.Id, inputModel.Image); //salva su disco
                    }
                    FormattableString queryUpdate = $@"
                    UPDATE Courses
                    SET
                    Title={inputModel.Title},
                    Description={inputModel.Description},
                    Email={inputModel.Email},
                    FullPrice_Amount={inputModel.FullPrice.Amount},
                    FullPrice_Currency={inputModel.FullPrice.Currency},
                    CurrentPrice_Amount={inputModel.CurrentPrice.Amount},
                    CurrentPrice_Currency={inputModel.CurrentPrice.Currency},
                    ImagePath=COALESCE({imagePath},ImagePath)
                    WHERE
                    Id ={inputModel.Id}
                    AND
                    RowVersion={inputModel.RowVersion}"; //COALESCE: se il primo valore è null, allora sceglie il secondo

                    int affectedRows = await db.CommandAsync(queryUpdate);
                    if (affectedRows == 0)
                    {
                         bool courseExists = await db.QueryScalarAsync<bool>($@"SELECT COUNT (*) FROM Courses WHERE Id={inputModel.Id};");
                         if (courseExists)
                         {
                              throw new OptimisticConcurrencyException();
                         }
                         else
                         {
                              throw new CourseNotFoundException(inputModel.Id);
                         }
                    }
               }
               catch (SqliteException excep) when (excep.SqliteErrorCode == 19)
               {
                    throw new CourseTitleUnavailableException("Titolo non disponibile", excep);
               }
               catch (ImagePersistenceException exc)//meglio non usare la magickExpcetion direttamente, dato che il serv appl deve essere debolmente accoppiato a questo servz infrastrtutturale
               {
                    throw new CourseImageInvalidException(inputModel.Id, exc);
               }
               CourseDetailViewModel result = await GetCourseAsync(inputModel.Id);
               return result;
          }
     }
}