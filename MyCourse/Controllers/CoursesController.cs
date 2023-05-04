using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.InputModels;
using MyCourse.Models.Services.Application;
using MyCourse.Models.ViewModels;

namespace MyCourse.Controllers
{
     public class CoursesController : Controller
     {
          private readonly ICourseService courseService;

          public CoursesController(ICachedCourseService courseService)
          {
               this.courseService = courseService;
          }
          public async Task<IActionResult> Index(CourseListInputModel model) //Sezione 13 - 91 - Model Binding personalizzato -  invece di passare le variabili una a una, è stata creata una classe con queste dentro, anche per aggiungere la sanitizzazione e altre utilità
          {
               ViewBag.Title = "Catalogo dei corsi";
               List<CourseViewModel> courses = await courseService.GetCoursesAsync(model);
               return View(courses);
          }

          public async Task<IActionResult> Detail(int id)
          {
               CourseDetailViewModel viewModel = await courseService.GetCourseAsync(id);
               ViewBag.Title = viewModel.Title;
               return View(viewModel);
          }
     }
}