using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Controllers;
using MyCourse.Models.Entities;
using MyCourse.Models.Enums;
using MyCourse.Models.ValueTypes;

namespace MyCourse.Models.InputModels
{
     public class CourseEditInputModel : IValidatableObject
     {
          /* CourseEditInputModel e CourseDetailViewModel sono molto simili
          Tuttavia non bisogna riutilizzare lo stesso viewmodel dato che hanno due scopi diversi, non è una best practice*/
          [Required]
          public int Id { get; set; }

          [Required(ErrorMessage = "Il titolo è obbligatorio"),
          MinLength(10, ErrorMessage = "Il titolo dev'essere di almeno {1} caratteri"),
          MaxLength(100, ErrorMessage = "Il titolo deve avere meno di {1} caratteri"),
          RegularExpression(@"^[\w\s\.]+$", ErrorMessage = "Titolo non valido"),
          Remote(action: nameof(CoursesController.IsTitleAvailable), controller: "Courses", ErrorMessage = "Il titolo già esiste nel database")]
          public string Title { get; set; }

          [MinLength(10, ErrorMessage = "La descrizione dev'essere di almeno {1} caratteri"),
          MaxLength(1000, ErrorMessage = "La descrizione deve avere meno di {1} caratteri"),
          Display(Name = "Descrizione")] //il NAME andrà scritto nella view, nel tag LABEL con l'attributo asp-for Description, come risultato avremo una label con attrb for
          public string Description { get; set; }
          public string ImagePath { get; set; }
          public string Email { get; set; }
          public Money FullPrice { get; set; }
          public Money CurrentPrice { get; set; }

          public static CourseEditInputModel FromDataRecord(DataRow courseRow)
          {
               var courseEditInputModel = new CourseEditInputModel
               {
                    Id = Convert.ToInt32(courseRow["Id"]),
                    Title = Convert.ToString(courseRow["Title"]),
                    ImagePath = Convert.ToString(courseRow["ImagePath"]),
                    Description = Convert.ToString(courseRow["Description"]),
                    Email = Convert.ToString(courseRow["Email"]),
                    FullPrice = new Money(
                        Enum.Parse<Currency>(Convert.ToString(courseRow["FullPrice_Currency"])),
                        Convert.ToDecimal(courseRow["FullPrice_Amount"])
                    ),
                    CurrentPrice = new Money(
                        Enum.Parse<Currency>(Convert.ToString(courseRow["CurrentPrice_Currency"])),
                        Convert.ToDecimal(courseRow["CurrentPrice_Amount"])
                    ),
               };
               return courseEditInputModel;
          }
          public static CourseEditInputModel FromEntity(Course course)
          {
               return new CourseEditInputModel
               {
                    Id = course.Id,
                    Title = course.Title,
                    ImagePath = course.ImagePath,
                    Email=course.Email,
                    CurrentPrice = course.CurrentPrice,
                    FullPrice = course.FullPrice
               };
          }
          /*
          Usando interfaccia IValidatableObject possiamo fare delle validazione più complesse
          Penso che non usiamo un remote action come in caso del titolo perché quello fa un controllo sul database
          Questo è solo un confronto tra valori all'interno della classe e dentro l'input model meglio non scrivere logica che si rivolge al db (credo eh)
          */
          public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
          {
               if (FullPrice.Currency != CurrentPrice.Currency)
               {
                    yield return new ValidationResult("Il prezzo intero deve avere la stessa valuta di quello attuale", new[] { nameof(FullPrice), nameof(CurrentPrice) });
               }
               else if (FullPrice.Amount < CurrentPrice.Amount)
               {
                    yield return new ValidationResult("Il prezzo intero non può essere minore a quello attuale", new[] { nameof(FullPrice) });
               }
          }
     }
}