using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyCourse.Models.Enums;
using MyCourse.Models.ValueTypes;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Application
{
     public class CourseService
     {
          public List<CourseViewModel> GetServices()
          {
          var courseList = new List<CourseViewModel>();
            var rand = new Random();
            for (int i = 1; i <= 20; i++)
            {
                var price = Convert.ToDecimal(rand.NextDouble() * 10 + 10);
                var course = new CourseViewModel
                {
                    Id = i,
                    Title = $"Corso {i}",
                    CurrentPrice = new Money(Currency.EUR, price),
                    FullPrice = new Money(Currency.EUR, rand.NextDouble() > 0.5 ? price : price + 1),
                    Author = "Nome Cognome utente n"+i.ToString(),
                    Rating = rand.Next(10, 50) / 10.0,
                    ImagePath = "/doge.jpg"
                };
                courseList.Add(course);
            }
            return courseList;
          }
     }
}