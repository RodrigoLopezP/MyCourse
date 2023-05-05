using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyCourse.Models.Entities;
using MyCourse.Models.InputModels;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Application
{
    public class EfCoreCourseService : ICourseService
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

        public async Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel inputFromViews)
        {
            #region IQueryable di base, per applicare i primi filtri se presenti
            IQueryable<Course> baseQuery = dbContext.Courses;
            switch (inputFromViews.OrderBy)
            {
                case "Title":
                    if (inputFromViews.Ascending)
                    {
                        baseQuery = baseQuery.OrderBy(course => course.Title);
                    }
                    else
                    {
                        baseQuery = baseQuery.OrderByDescending(course => course.Title);
                    }
                    break;
                case "Rating":
                    if (inputFromViews.Ascending)
                    {
                        baseQuery = baseQuery.OrderBy(course => course.Rating);
                    }
                    else
                    {
                        baseQuery = baseQuery.OrderByDescending(course => course.Rating);
                    }
                    break;
                case "CurrentPrice":
                    if (inputFromViews.Ascending)
                    {
                        baseQuery = baseQuery.OrderBy(course => course.CurrentPrice.Amount);
                    }
                    else
                    {
                        baseQuery = baseQuery.OrderByDescending(course => course.CurrentPrice.Amount);
                    }
                    break;
                case "Id":
                    if (inputFromViews.Ascending)
                    {
                        baseQuery = baseQuery.OrderBy(course => course.Id);
                    }
                    else
                    {
                        baseQuery = baseQuery.OrderByDescending(course =>  course.Id);
                    }
                    break;
            }
            #endregion

            #region IQueryable con la quale prendiamo tutti i risultati della intera tabella
            IQueryable<CourseViewModel> queryEF = baseQuery
            .AsNoTracking()//EF no farà il log tracking, utile per aumentare le prestazione. Usare solo se facciamo delle SELECT
            .Where(x => x.Title.ToLower().Contains(inputFromViews.Search.ToLower()))
            .Select(course => new CourseViewModel
            {
                Id = course.Id,
                Title = course.Title,
                ImagePath = course.ImagePath,
                Author = course.Author,
                Rating = course.Rating,
                CurrentPrice = course.CurrentPrice,
                FullPrice = course.FullPrice,
            });

            #endregion
            //Con ToList eseguiamo effettivamente la query sul db per ottenere il risultato
            List<CourseViewModel> courses = await queryEF
                    .Skip(inputFromViews.Offset)
                    .Take(inputFromViews.Limit).ToListAsync(); //Skup e TAKE li portiamo qui perche queryEF ci servirà intera sotto per il count

            int totCourses = await queryEF.CountAsync();

            ListViewModel<CourseViewModel> result = new ListViewModel<CourseViewModel>
            {
                Results = courses,
                TotalCount = totCourses
            };

            return result;
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
    }
}