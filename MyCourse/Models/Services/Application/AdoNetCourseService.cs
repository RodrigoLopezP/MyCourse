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
        private readonly IOptionsMonitor<CoursesOptions> coursesOpts;
        /*Lez-12-72 - IOptionsMonitor<CoursesOptions> coursesOptions 
        *-Leggere la configurazione del appsetting.json in modo tipizzato */
        /*Lez-12-72 - IOptionsMonitor<CoursesOptions> coursesOptions 
        *-Leggere la configurazione del appsetting.json in modo tipizzato */
        private readonly ILogger<AdoNetCourseService> _logger;
        public AdoNetCourseService(ILogger<AdoNetCourseService> logger, IDatabaseAccessor db, IOptionsMonitor<CoursesOptions> coursesOptions)
        {
            _logger = logger;
            this.coursesOpts = coursesOptions;
            this.db = db;
        }

        public Task<List<CourseViewModel>> GetBestRatingCoursesAsync()
        {
            throw new NotImplementedException();
        }
        public Task<List<CourseViewModel>> GetMostRecentCoursesAsync()
        {
            throw new NotImplementedException();
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
                _logger.LogWarning("Course {id} non trovato", id);
                throw new CourseNotFoundException(id);
            }
            var courseRow = courseTable.Rows[0];
            /*
            Nella riga sotto inizio ad assegnare i valore al CourseDetailViewModel, quelli ereditati da CourseViewModel
            con i valori ottenuti da questa prima SELECT
            Nella seconda SELECT vengono salvati anche i dettagli per ogni lezione
            */
            CourseDetailViewModel courseDetailViewModel = CourseDetailViewModel.FromDataRow(courseRow);

            //Lessons
            var lessonsTable = dataSet.Tables[1];
            foreach (DataRow lessonRow in lessonsTable.Rows)
            {
                LessonViewModel lessonViewModel = LessonViewModel.FromDataRow(lessonRow);
                courseDetailViewModel.Lessons.Add(lessonViewModel);
            }

            return courseDetailViewModel;
        }

        public async Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel courseFilters)
        {
            string trueOrderBy = courseFilters.OrderBy == "CurrentPrice" ? "CurrentPrice_Amount" : courseFilters.OrderBy;
            string direction = courseFilters.Ascending ? "ASC" : "DESC"; // se ascending è TRUE, allora diventa direction diventa ASC
            
            FormattableString query = $@"
            SELECT Courses.id, Courses.Title, Courses.ImagePath, Courses.Author, Courses.Rating, Courses.FullPrice_Amount, Courses.FullPrice_Currency, Courses.CurrentPrice_Amount, Courses.CurrentPrice_Currency
            FROM Courses
            WHERE Courses.Title
            LIKE {"%" + courseFilters.Search + "%"}
            ORDER BY {(Sql)trueOrderBy} {(Sql)direction}
            LIMIT {courseFilters.Limit} OFFSET {courseFilters.Offset};
            SELECT COUNT(*)
            FROM Courses
            WHERE Courses.Title LIKE {"%"+courseFilters.Search+"%"}";
            
            DataSet dataSet = await db.QueryAsync(query);
            var dataTable = dataSet.Tables[0];
            var courseList = new List<CourseViewModel>();
            foreach (DataRow courseRow in dataTable.Rows)
            {
                CourseViewModel course = CourseViewModel.FromDataRow(courseRow);
                courseList.Add(course);
            }

            ListViewModel<CourseViewModel> result = new ListViewModel<CourseViewModel>{
                Results=courseList,
                TotalCount=Convert.ToInt32(dataSet.Tables[1].Rows[0][0])
            };
            return result;
        }


    }
}