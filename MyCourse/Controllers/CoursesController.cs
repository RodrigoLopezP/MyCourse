using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MyCourse.Controllers
{
    public class CoursesController:Controller
    {
        public IActionResult Index() 
        {
            return Content("Sono index");
        }

        public IActionResult Detail(string id) 
        {
            return Content($"Sono Detail, ho ricevuto un id uguale a {id}");
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