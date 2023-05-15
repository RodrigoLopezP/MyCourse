using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MyCourse.Models.InputModels
{
    public class CourseCreateInputModel
    {
        [Required(ErrorMessage ="Il titolo è obbligatori"),
        MinLength(10, ErrorMessage ="La lunghezza minima è {1}"),
        MaxLength(100, ErrorMessage ="La lunghezza massima è di {1}"),
        RegularExpression(@"^[\w\s\.]+$", ErrorMessage="Caratteri speciali non sono ammessi")
        ]
        public string Title { get; set; }
    }
}