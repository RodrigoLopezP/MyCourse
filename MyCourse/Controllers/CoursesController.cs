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
        public async Task<IActionResult> Index(CourseListInputModel inputFromViews) //Sezione 13 - 91 - Model Binding personalizzato -  invece di passare le variabili una a una, è stata creata una classe con queste dentro, anche per aggiungere la sanitizzazione e altre utilità
        {
            ViewBag.Title = "Catalogo dei corsi";
            ListViewModel<CourseViewModel> courses = await courseService.GetCoursesAsync(inputFromViews);

            CourseListViewModel ciao = new CourseListViewModel
            {
                Courses = courses,
                Input = inputFromViews,
            };

            return View(ciao);
        }

        public async Task<IActionResult> Detail(int id)
        {
            CourseDetailViewModel viewModel = await courseService.GetCourseAsync(id);
            ViewBag.Title = viewModel.Title;
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CourseCreateInputModel nuovoCorso) //qui finisce la stessa pagina ma l'utente ha messo il nome del corso, e c'è da chiamare il servizio applicativo (ef core, ado net boh)
        {
            ViewBag.Title="Create";
            CourseDetailViewModel x = await courseService.CreateCourseAsync(nuovoCorso);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Create() //mostrare form all'utente e basta
        {
            ViewBag.Title="Create";
            var inModel= new CourseCreateInputModel();
            return View(inModel);
        }
    }
}