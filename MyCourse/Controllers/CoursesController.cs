using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
          public async Task<IActionResult> Index(
                                          int page, //lo si trova nella view del Courses con l'asp-route-parolaCheVuoi
                                          string search = null,
                                          string orderBy = "title",
                                          bool ascending = true)
          {
               ViewBag.Title = "Catalogo dei corsi";
               List<CourseViewModel> courses = await courseService.GetCoursesAsync(search,page,orderBy, ascending);
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