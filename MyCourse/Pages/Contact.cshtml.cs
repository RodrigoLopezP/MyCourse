using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyCourse.Models.Services.Application.Courses;
using MyCourse.Models.ViewModels;

namespace MyCourse.Pages
{
     public class ContactModel : PageModel
     {
          public CourseDetailViewModel Course { get; private set; }

          [Required(ErrorMessage = "Il testo della domanda è obbligatorio")]
          [Display(Name = "La tua domanda")]
          [BindProperty]
          public string Question { get; set; }

          public async Task<IActionResult> OnGetAsync(int id, [FromServices] ICourseService courseService)
          {
               try
               {
                    Course = await courseService.GetCourseAsync(id);
                    ViewData["Title"] = $"Invia una domanda";
                    return Page();
               }
               catch
               {
                    return RedirectToAction("Index", "Courses");
               }
          }

          public async Task<IActionResult> OnPostAsync(int id, [FromServices] ICourseService courseService)
          {
               if (ModelState.IsValid)
               {
                     //invio messaggino al docente
                    await courseService.SendQuestionToCourseAuthorAsync(id, Question);
                    TempData["ConfirmationMessage"] = "La tua domanda è stata inviata";
                    return RedirectToAction("Detail", "Courses", new { id = id });
               }
               else
               {
                    return await OnGetAsync(id, courseService);
               }
          }
     }
}