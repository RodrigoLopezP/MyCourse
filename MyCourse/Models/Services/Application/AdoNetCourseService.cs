using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Application
{
     public class AdoNetCourseService : ICourseService
     {
        private readonly IDatabaseAccessor db;
        public AdoNetCourseService(IDatabaseAccessor db)
        {
            this.db=db;
        }
          public async Task<CourseDetailViewModel> GetCourseAsync(int id)
          {
               FormattableString query =$@"SELECT Id, Title, Description, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Courses WHERE Id={id}
               ; SELECT Id, Title, Description, Duration FROM Lessons WHERE CourseId={id}";
               DataSet dataSet=await db.QueryAsync(query);


               //Course
               var courseTable= dataSet.Tables[0];
               if (courseTable.Rows.Count!=1)
               {
                    throw new InvalidOperationException($"Non è ritornata esattamente 1 riga durante ottenimento del corso con id {id}'");
               }
               var courseRow = courseTable.Rows[0];
               /*
               Nella riga sotto inizio ad assegnare i valore al CourseDetailViewModel, quelli ereditati da CourseViewModel
               con i valori ottenuti da questa prima SELECT
               Nella seconda SELECT vengono salvati anche i dettagli per ogni lezione
               */
               CourseDetailViewModel courseDetailViewModel= CourseDetailViewModel.FromDataRow(courseRow);

               //Lessons
               var lessonsTable= dataSet.Tables[1];
               foreach (DataRow lessonRow in lessonsTable.Rows)
               {
                    LessonViewModel lessonViewModel=LessonViewModel.FromDataRow(lessonRow);
                    courseDetailViewModel.Lessons.Add(lessonViewModel);
               }

               return courseDetailViewModel;
          }

          public async Task<List<CourseViewModel>> GetCoursesAsync()
          {
               FormattableString query=$"SELECT Courses.id, Courses.Title, Courses.ImagePath, Courses.Author, Courses.Rating, Courses.FullPrice_Amount, Courses.FullPrice_Currency, Courses.CurrentPrice_Amount, Courses.CurrentPrice_Currency  FROM Courses";
               DataSet dataSet= await db.QueryAsync(query);
               var dataTable= dataSet.Tables[0];
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