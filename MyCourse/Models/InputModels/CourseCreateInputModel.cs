using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Controllers;

namespace MyCourse.Models.InputModels
{
    public class CourseCreateInputModel
    //queste data annotation scattano al momento del Model Binding
    {
        [Required(ErrorMessage ="Il titolo è obbligatorio"),
        MinLength(10, ErrorMessage ="La lunghezza minima è {1}"),
        MaxLength(100, ErrorMessage ="La lunghezza massima è di {1}"),
        RegularExpression(@"^[\w\s\.]+$", ErrorMessage="Caratteri speciali non sono ammessi"),
        Remote(action:nameof(CoursesController.IsTitleAvailable), controller:"Courses", ErrorMessage ="Controllo da client: il titolo esiste di già")//questo serve per il controllo dell'esistenza del nome mentre l'utente digita nell'input text, quindi controllo da client
        ]
        public string Title { get; set; }
    }
}