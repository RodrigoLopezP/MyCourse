using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Logging;
using MyCourse.Models.Exceptions;
using MyCourse.Models.Exceptions.Application;

namespace MyCourse.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            switch (feature.Error)
            {
                case CourseNotFoundException exc:
                    ViewBag.Title = "Corso non trovato";
                    Response.StatusCode = 404;
                    return View("CourseNotFound");
                case UserUnknownException exc:
                    ViewBag.Title = "Utente sconosciuto";
                    Response.StatusCode = 400;
                    return View();
                case CourseSubscriptionException exc:
                    ViewData["Title"] = "Non è stato possibile iscriverti al corso";
                    Response.StatusCode = 400;
                    return View();
                default:
                    ViewBag.Title = "Error";
                    return View();
            }

        }
    }
}