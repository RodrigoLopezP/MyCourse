using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Application
{
    public interface ICourseService
    {
        Task<List<CourseViewModel>> GetCoursesAsync(string search, int page, string orderBy, bool ascending);
        Task<CourseDetailViewModel> GetCourseAsync(int id);
    }
}