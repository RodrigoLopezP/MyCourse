using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyCourse.Models.InputModels;

namespace MyCourse.Models.ViewModels
{
    public class CourseListViewModel
    {
        public List<CourseViewModel> Courses { get; set; } // Models/ViewModels/CourseViewModel.cs
        public CourseListInputModel Input { get; set; }    // Models/InputModels/CourseListInputModel.cs
    }
}