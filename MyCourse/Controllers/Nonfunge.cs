using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MyCourse.Controllers
{
    public class Nonfunge:Controller
    {
        public IActionResult Index() 
        {
            return Content("Questo file nella cartella controller non dovrebbe funzionare, dato che non ha il sufisso CONTROLLER");
        }
        public IActionResult test(string id) 
        {
            return Content($"Questo non dovrebbe funzionare, id di input => {id} ");
        }
    }
}