using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MyCourse.Controllers
{
    public class AmongUsController:Controller
    {
        public IActionResult Index() 
        {
            ViewBag.Title="AmongUs ඞ";
            return View();
        }

        public IActionResult Sus(string id) 
        {
            return Content($"L'amongus ha sussato circa {id} volte");
        }
    }
}