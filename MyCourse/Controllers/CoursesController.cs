using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Options;
using MyCourse.Models.Enums;
using MyCourse.Models.Exceptions;
using MyCourse.Models.InputModels;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Application.Courses;
using MyCourse.Models.Services.Application.Lessons;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ValueTypes;
using MyCourse.Models.ViewModels;
using MyCourse.Models.ViewModels.Courses;

namespace MyCourse.Controllers
{
     public class CoursesController : Controller
     {
          private readonly ICourseService courseService;

          public CoursesController(ICachedCourseService courseService)
          {
               this.courseService = courseService;
          }
          [AllowAnonymous]
          public async Task<IActionResult> Index(CourseListInputModel inputFromViews) //Sezione 13 - 91 - Model Binding personalizzato -  invece di passare le variabili una a una, è stata creata una classe con queste dentro, anche per aggiungere la sanitizzazione e altre utilità
          {
               ViewBag.Title = "Catalogo dei corsi";
               ListViewModel<CourseViewModel> courses = await courseService.GetCoursesAsync(inputFromViews);

               CourseListViewModel ciao = new()
               {
                    Courses = courses,
                    Input = inputFromViews,
               };

               return View(ciao);
          }
          
          [Authorize(Policy = nameof(Policy.CourseViewer))]
          public async Task<IActionResult> Detail(int id)
          {
               CourseDetailViewModel viewModel = await courseService.GetCourseAsync(id);
               ViewBag.Title = viewModel.Title;
               return View(viewModel);
          }
          [Authorize(Roles = nameof(Role.Teacher))]
          [HttpPost]
          public async Task<IActionResult> Create(CourseCreateInputModel nuovoCorso,
                                                  [FromServices] IAuthorizationService authorizationService,
                                                  [FromServices] IEmailClient emailClient,
                                                  [FromServices] IOptionsMonitor<UsersOptions> usersOptions) //qui finisce la stessa pagina ma l'utente ha messo il nome del corso, e c'è da chiamare il servizio applicativo (ef core, ado net boh)
          {
               if (ModelState.IsValid)
               {
                    try
                    {
                         CourseDetailViewModel x = await courseService.CreateCourseAsync(nuovoCorso);

                         AuthorizationResult result = await authorizationService.AuthorizeAsync(User, nameof(Policy.CourseLimit));
                         if (!result.Succeeded)
                         {
                              await emailClient.SendEmailAsync(usersOptions.CurrentValue.NotificationEmailRecipient, "Avviso superamento soglia", $"Il docente {User.Identity.Name} ha creato molti corsi: verifica che riesca a gestirli tutti.");
                         }

                         TempData["ConfirmationMessage"] = "Corso creato! Vuoi aggiungere qualche dato in più? Ma sì, dai";
                         return RedirectToAction(nameof(Edit), new { id = x.Id }); //creato il nuovo corso, ti fa andare alla finestra di edit
                    }
                    catch (CourseTitleUnavailableException)
                    {
                         ModelState.AddModelError(nameof(CourseDetailViewModel.Title), "Questo titolo già esiste"); //nel primo parametro basta scrivere "Title", ma penso che così sia più bellino, cioè, più preciso
                    }
               }
               ViewBag.Title = "Create";

               return View(nuovoCorso);
          }
          [Authorize(Roles = nameof(Role.Teacher))]
          [HttpGet]//opzione di default
          public IActionResult Create() //mostrare form all'utente e basta
          {
               ViewBag.Title = "Create";
               var inModel = new CourseCreateInputModel();
               return View(inModel);
          }
          //E' un controllo che viene usato durante il ModelBinding di un ogg CourseCreateInputModel
          [Authorize(Roles = nameof(Role.Teacher))]
          public async Task<IActionResult> IsTitleAvailable(string title, int id = 0)
          {
               bool result = await courseService.IsTitleAvailableAsync(title, id);
               return Json(result);
          }

          /* Prima va in Edit http get perché gli passiamo un id, lui ci risputa il inputModel perché noi possimoa
          modificarlo.
          Salvando i dati da modifiche, si finisce nel HttpPost 
          */
          [HttpGet]
          [Authorize(Policy = nameof(Policy.CourseAuthor))]//Controllare in Model/Authorization/...Handler
          [Authorize(Roles = nameof(Role.Teacher))]
          public async Task<IActionResult> Edit(int id)
          {
               ViewBag.Title = "Edit";
               var inputModel = await courseService.GetCourseForEditingAsync(id);
               return View(inputModel);
          }
          [HttpPost]
          [Authorize(Policy = nameof(Policy.CourseAuthor))]
          [Authorize(Roles = nameof(Role.Teacher))]
          public async Task<IActionResult> Edit(CourseEditInputModel inputModel)
          {
               if (ModelState.IsValid)
               {
                    try
                    {
                         CourseDetailViewModel course = await courseService.EditCourseAsync(inputModel);
                         TempData["ConfirmationMessage"] = "I dati sono stati salvati con successo";//i TEMPDATA rimango presenti quando fai un redirect
                         return RedirectToAction(nameof(Detail), new { id = inputModel.Id });//Torna alla descrizione del corso
                    }
                    catch (CourseTitleUnavailableException)
                    {
                         ModelState.AddModelError(nameof(CourseEditInputModel.Title), "Questo titolo già esiste"); //nel primo parametro basta scrivere "Title", ma penso che così sia più bellino, cioè, più preciso
                    }
                    catch (CourseImageInvalidException)
                    {
                         ModelState.AddModelError(nameof(CourseEditInputModel.Image), "L'immagine selezionata non è valida, usare un'altra");
                    }
                    catch (OptimisticConcurrencyException)
                    {
                         ModelState.AddModelError("", "Corso modificato di recente, ricaricare la pagina per effettutare le modifiche");
                    }
               }

               ViewBag.Title = "Edit";
               return View(inputModel);
          }
          [HttpPost]
          [Authorize(Policy = nameof(Policy.CourseAuthor))]
          [Authorize(Roles = nameof(Role.Teacher))]
          public async Task<IActionResult> Delete(CourseDeleteInputModel inputModel)
          {
               await courseService.DeleteCourseAsync(inputModel);
               TempData["ConfirmationMessage"] = "Il corso è stato eliminato ma potrebbe continuare a comparire negli elenchi per un breve periodo, finché la cache non viene aggiornata.";
               return RedirectToAction(nameof(Index));
          }
          //il TOKEN viene restituito dalla pagina di pagamento. Paypal lo fa in automatico, a Stripe bisogna chiederlo esplicitamente
          [Authorize]
          public async Task<IActionResult> Subscribe(int id, string token)
          {
               if (!ModelState.IsValid)
               {
                    return View();
               }
               CourseSubscribeInputModel inputModel = await courseService.CapturePaymentAsync(id, token);
               await courseService.SubscribeCourseAsync(inputModel);

               TempData["ConfirmationMessage"] = "Grazie per esserti iscritto, guarda subito la prima lezione!";
               return RedirectToAction(nameof(Detail), new { id = id });
          }
          [Authorize]
          public async Task<IActionResult> Pay(int id)
          {
               string paymentUrl = await courseService.GetPaymentUrlAsync(id);
               return Redirect(paymentUrl);
          }
          [Authorize(Policy = nameof(Policy.CourseSubscriber))]
          public async Task<IActionResult> Vote(int id)
          {
               CourseVoteInputModel inputModel = new()
               {
                    Id = id,
                    Vote = await courseService.GetCourseVoteAsync(id) ?? 0
               };

               return View(inputModel);
          }

          [Authorize(Policy = nameof(Policy.CourseSubscriber))]
          [HttpPost]
          public async Task<IActionResult> Vote(CourseVoteInputModel inputModel)
          {
               await courseService.VoteCourseAsync(inputModel);
               TempData["ConfirmationMessage"] = "Grazie per aver votato!";
               return RedirectToAction(nameof(Detail), new { id = inputModel.Id });
          }

          [Authorize]
          public async Task<IActionResult> Personal()
          {
               string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

               PersonalCoursesViewModel viewModel = new()
               {
                    AuthoredCourses = await courseService.GetCoursesByAuthorAsync(userId),
                    SubscribedCourses = await courseService.GetCoursesBySubscriberAsync(userId)
               };

               ViewData["Title"] = "I miei corsi";
               return View(viewModel);
          }
     }
}
