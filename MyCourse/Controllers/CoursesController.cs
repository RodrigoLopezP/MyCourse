using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.Services.Application;
using MyCourse.Models.ViewModels;

namespace MyCourse.Controllers
{
    public class CoursesController:Controller
    {
       private readonly ICourseService courseService;

        public CoursesController(ICourseService courseService)
        {
            this.courseService=courseService;
        }
        public IActionResult Index() 
        {
            ViewBag.Title="Catalogo dei corsi";
            List<CourseViewModel> courses=courseService.GetCourses();
            return View(courses);
        }

        public IActionResult Detail(int id) 
        {
            CourseDetailViewModel viewModel= courseService.GetCourse(id);
            ViewBag.Title=viewModel.Title;
            return View(viewModel);
        }
    }
}