using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
            CourseDetailViewModel dettaglioCorso=null;

            //Course
            FormattableString courseQuery = $"SELECT Id, Title, Description, Duration FROM Lessons WHERE CourseId={id}";
            IAsyncEnumerable<IDataRecord> courseResults=db.QueryAsync(courseQuery);
            await foreach (IDataRecord dataRecord in courseResults)
            {
                dettaglioCorso=CourseDetailViewModel.FromDataRecord(dataRecord);
                break;
            }
            if (dettaglioCorso==null)
            {
                _logger.LogWarning("Course {id} non trovato", id);
                throw new CourseNotFoundException(id);
            }
 
            //Lessons
            FormattableString lessonsQuery = $"SELECT Id, Title, Description, Duration FROM Lessons WHERE CourseId={id}";
            IAsyncEnumerable<IDataRecord> lessonResults=db.QueryAsync(lessonsQuery);
            await foreach (IDataRecord dataRecord in lessonResults)
            {
                LessonViewModel lessonViewModel = LessonViewModel.FromDataRecord(dataRecord);
                dettaglioCorso.Lessons.Add(lessonViewModel);
            }

            return dettaglioCorso;
        }

        public async Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel coursesFilters)
        {
            string trueOrderBy = coursesFilters.OrderBy == "CurrentPrice" ? "CurrentPrice_Amount" : coursesFilters.OrderBy;
            string direction = coursesFilters.Ascending ? "ASC" : "DESC"; // se ascending è TRUE, allora diventa direction diventa ASC

            FormattableString query = $@"
            SELECT Courses.id, Courses.Title, Courses.ImagePath, Courses.Author, Courses.Rating, Courses.FullPrice_Amount, Courses.FullPrice_Currency, Courses.CurrentPrice_Amount, Courses.CurrentPrice_Currency
            FROM Courses
            WHERE Courses.Title
            LIKE {"%" + coursesFilters.Search + "%"}
            ORDER BY {(Sql)trueOrderBy} {(Sql)direction}
            LIMIT {coursesFilters.Limit} OFFSET {coursesFilters.Offset}";

            var courseList = new List<CourseViewModel>();
            IAsyncEnumerable<IDataRecord> coursesResult = db.QueryAsync(query);

            await foreach (IDataRecord dataRecord in coursesResult)
            {
                CourseViewModel course = CourseViewModel.FromDataRecord(dataRecord);
                courseList.Add(course);
            }

            int count = 0;
            FormattableString countQuery = $@" SELECT COUNT(*)
                                            FROM Courses                
                                            WHERE Courses.Title LIKE {"%" + coursesFilters.Search + "%"}";
            IAsyncEnumerable <IDataRecord> countResults=db.QueryAsync(countQuery);
            await foreach (IDataRecord dataRecord in countResults)
            {
                count=dataRecord.GetInt32(0);
                break;
            }


            ListViewModel<CourseViewModel> result = new ListViewModel<CourseViewModel>
            {
                Results = courseList,
                TotalCount = count
            };
            return result;
        }


    }
}