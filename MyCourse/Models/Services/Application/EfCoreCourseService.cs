using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Application
{
    public class EfCoreCourseService : ICourseService
    {
        private readonly MyCourseDbContext dbContext;

        public EfCoreCourseService(MyCourseDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<CourseDetailViewModel> GetCourseAsync(int id)
        {
            CourseDetailViewModel courseDetail= await
            dbContext.Courses
            .Where(course => course.Id==id)
            .AsNoTracking()//EF no farà il log tracking, utile per aumentare le prestazione. Usare solo se facciamo delle SELECT
            .Select( course=> new CourseDetailViewModel{
                Id = course.Id,
                Title=course.Title,
                ImagePath=course.ImagePath,
                Author=course.Author,
                Rating = course.Rating,
                CurrentPrice=course.CurrentPrice,
                FullPrice=course.FullPrice,

                Description=course.Description,
                Lessons=course.Lessons.Select(lesson => new LessonViewModel{
                    Id = lesson.Id,
                    Title=lesson.Title,
                    Description=lesson.Description,
                    Duration=lesson.Duration,
                }).ToList(), //qui non server ASYNC perché interagiamo effettivamente con il db con .SINGLEASYNC()
            })
            .SingleAsync(); // restituisce 1 elem, se ci sono 0 o più di uno = ECCEZIONE
            //.firstAsync();// restituisce primo elem, se ci sono più di uno OK, raccatta comunque il primo
            //.singleODefaultAsync(); //Come singleAsync, ma se è VUOTO va bene, dà NULL
            //.FirstOrDefaultAxync() // se è 1 solo ok, se è vuoto ok, se è più di 1 raccatta il primo. E' IL PIU' TOLLERANTE

            return courseDetail;
        }

        public async Task<List<CourseViewModel>> GetCoursesAsync()
        {
            List<CourseViewModel> courses= await dbContext.Courses
            .AsNoTracking()//EF no farà il log tracking, utile per aumentare le prestazione. Usare solo se facciamo delle SELECT
            .Select(course => new CourseViewModel{
                Id = course.Id,
                Title=course.Title,
                ImagePath=course.ImagePath,
                Author=course.Author,
                Rating = course.Rating,
                CurrentPrice=course.CurrentPrice,
                FullPrice=course.FullPrice,
            }).ToListAsync();
            return courses;
        }
    }
}