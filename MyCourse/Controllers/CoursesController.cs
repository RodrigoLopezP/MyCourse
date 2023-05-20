using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.Exceptions;
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
               if (ModelState.IsValid)
               {
                    try
                    {
                         CourseDetailViewModel x = await courseService.CreateCourseAsync(nuovoCorso);
                         return RedirectToAction(nameof(Index)); //creato il nuovo corso, ti fa tornare alla lista di corsi (per ora)
                    }
                    catch (CourseTitleUnavailableException)
                    {
                         ModelState.AddModelError(nameof(CourseDetailViewModel.Title), "Questo titolo già esiste"); //nel primo parametro basta scrivere "Title", ma penso che così sia più bellino, cioè, più preciso
                    }
               }
               ViewBag.Title = "Create";

               return View(nuovoCorso);
          }

          [HttpGet]
          public IActionResult Create() //mostrare form all'utente e basta
          {
               ViewBag.Title = "Create";
               var inModel = new CourseCreateInputModel();
               return View(inModel);
          }

          public async Task <IActionResult> IsTitleAvailable(string title) { 
            bool result= await courseService.IsTitleAvailableAsync(title);
            return Json(result);
           }
     }
}