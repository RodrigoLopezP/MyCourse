using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<CourseViewModel> BestRatingCourses { get; set; }
        public List<CourseViewModel> MostRecentCourses { get; set; }
    }
}