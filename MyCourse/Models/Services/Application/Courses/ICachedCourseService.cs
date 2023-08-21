using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Application.Courses
{
     public interface ICachedCourseService : ICourseService
     {
          /* Sez 12 - 79 Servizio cache 
          Questa interfaccia eredita i metodi di ICourseService, senza aggiungerne dei nuovi
          del resto deve fare le stesse cose (per ora)
          */
     }
}