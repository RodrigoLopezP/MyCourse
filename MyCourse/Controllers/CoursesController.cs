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
        public IActionResult Index() 
        {
            var courseService=new CourseService();
            List<CourseViewModel> courses=courseService.GetCourses();
            return View(courses);
        }

        public IActionResult Detail(int id) 
        {
            var courseService = new CourseService();
            CourseDetailViewModel viewModel= courseService.GetCourse(id);
            return View();
        }

        public IActionResult Doge(string anni) 
        {
            return Content($"Il doge ha esattamente {anni} anni -> non funziona perché la var di input è 'anni' e non 'id'");
        }

        public IActionResult Puggo(string id) 
        {
            return Content($"Il puggo ha esattamente {id} anni");
        }
    }
}