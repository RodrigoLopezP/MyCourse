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
          private readonly IOptionsMonitor<CoursesOptions> _coursesOpts;
          /*Lez-12-72 - IOptionsMonitor<CoursesOptions> coursesOptions 
          *-Leggere la configurazione del appsetting.json in modo tipizzato */
          /*Lez-12-72 - IOptionsMonitor<CoursesOptions> coursesOptions 
          *-Leggere la configurazione del appsetting.json in modo tipizzato */
          private readonly ILogger<AdoNetCourseService> _logger;
          public AdoNetCourseService(ILogger<AdoNetCourseService> logger, IDatabaseAccessor db, IOptionsMonitor<CoursesOptions> coursesOptions)
          {
               _logger = logger;
               this._coursesOpts = coursesOptions;
               this.db = db;
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

               FormattableString query = $@"SELECT Id, Title, Description, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Courses WHERE Id={id}
            ; SELECT Id, Title, Description, Duration FROM Lessons WHERE CourseId={id}";

               DataSet dataSet = await db.QueryAsync(query);

               //Course
               var courseTable = dataSet.Tables[0];
               if (courseTable.Rows.Count != 1)
               {
                    _logger.LogWarning("Course {id} not found", id);
                    throw new CourseNotFoundException(id);
               }
               var courseRow = courseTable.Rows[0];
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

               try
               {
                    DataSet dataSet = await db.QueryAsync($@"INSERT INTO Courses (Title, Author, ImagePath, CurrentPrice_Currency, CurrentPrice_Amount, FullPrice_Currency, FullPrice_Amount) VALUES ({title}, {author}, '/Courses/default.png', 'EUR', 0, 'EUR', 0);
                                                 SELECT last_insert_rowid();");

                    int courseId = Convert.ToInt32(dataSet.Tables[0].Rows[0][0]);
                    CourseDetailViewModel course = await GetCourseAsync(courseId);
                    return course;
               }
               catch (SqliteException exc) when (exc.SqliteErrorCode == 19)
               {
                    throw new CourseTitleUnavailableException(title, exc);
               }
          }
     }
}