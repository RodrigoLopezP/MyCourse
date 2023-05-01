using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCourse.Models.Exceptions;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ValueObjects;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Application
{
     public class AdoNetCourseService : ICourseService
     {
          private readonly IDatabaseAccessor db;
          private readonly IOptionsMonitor<CoursesOptions> coursesOpts;
          /*Lez-12-72 - IOptionsMonitor<CoursesOptions> coursesOptions 
          *-Leggere la configurazione del appsetting.json in modo tipizzato */
          /*Lez-12-72 - IOptionsMonitor<CoursesOptions> coursesOptions 
          *-Leggere la configurazione del appsetting.json in modo tipizzato */
          private readonly ILogger<AdoNetCourseService> _logger;
          public AdoNetCourseService(ILogger<AdoNetCourseService> logger, IDatabaseAccessor db, IOptionsMonitor<CoursesOptions> coursesOptions)
          {
               _logger = logger;
               this.coursesOpts = coursesOptions;
               this.db = db;
          }
          public async Task<CourseDetailViewModel> GetCourseAsync(int id)
          {
               _logger.LogInformation("Course {id} requested", id);

               FormattableString query = $@"SELECT Id, Title, Description, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Courses WHERE Id={id}
               ; SELECT Id, Title, Description, Duration FROM Lessons WHERE CourseId={id}";
               DataSet dataSet = await db.QueryAsync(query);

               //Course
               var courseTable = dataSet.Tables[0];
               if (courseTable.Rows.Count != 1)
               {
                    _logger.LogWarning("Course {id} non trovato", id);
                    throw new CourseNotFoundException(id);
               }
               var courseRow = courseTable.Rows[0];
               /*
               Nella riga sotto inizio ad assegnare i valore al CourseDetailViewModel, quelli ereditati da CourseViewModel
               con i valori ottenuti da questa prima SELECT
               Nella seconda SELECT vengono salvati anche i dettagli per ogni lezione
               */
               CourseDetailViewModel courseDetailViewModel = CourseDetailViewModel.FromDataRow(courseRow);

               //Lessons
               var lessonsTable = dataSet.Tables[1];
               foreach (DataRow lessonRow in lessonsTable.Rows)
               {
                    LessonViewModel lessonViewModel = LessonViewModel.FromDataRow(lessonRow);
                    courseDetailViewModel.Lessons.Add(lessonViewModel);
               }

               return courseDetailViewModel;
          }

          public async Task<List<CourseViewModel>> GetCoursesAsync(string search, int page, string orderBy, bool ascending)
          {
               page = Math.Max(1, page); // scegli il num maggiore fra questi due numeri
               //sanitizzazione dei valore per impaginare i corsi
               int limit = coursesOpts.CurrentValue.PerPage;
               int offset = (page - 1) * limit;
               //Sanitizzazione per order by - controlla se ha ricevuto un valore valido
               //e nel caso aggiunge le imposazione di default scritte nella configurazione
               var orderOptions = coursesOpts.CurrentValue.Order;
               if (!orderOptions.Allow.Contains(orderBy))
               {
                    orderBy = orderOptions.By;
                    ascending = orderOptions.Ascending;
               }
               //Sanitizzazione del prezzo
               if (orderBy == "CurrentPrice") //nel config è il valore "By"
               {
                    orderBy = "CurrentPrice_Amount"; //il nome vero della colonna nel DB
               }
               //Sanitizzazione ASC o DESC
               string direction = ascending ? "ASC" : "DESC"; // se ascending è TRUE, allora diventa direction diventa ASC
               FormattableString query = $"SELECT Courses.id, Courses.Title, Courses.ImagePath, Courses.Author, Courses.Rating, Courses.FullPrice_Amount, Courses.FullPrice_Currency, Courses.CurrentPrice_Amount, Courses.CurrentPrice_Currency FROM Courses WHERE Courses.Title LIKE {"%" + search + "%"} ORDER BY {(Sql)orderBy} {(Sql)direction} LIMIT {limit} OFFSET {offset}";
               DataSet dataSet = await db.QueryAsync(query);
               var dataTable = dataSet.Tables[0];
               var courseList = new List<CourseViewModel>();
               foreach (DataRow courseRow in dataTable.Rows)
               {
                    CourseViewModel course = CourseViewModel.FromDataRow(courseRow);
                    courseList.Add(course);
               }
               return courseList;
          }
     }
}