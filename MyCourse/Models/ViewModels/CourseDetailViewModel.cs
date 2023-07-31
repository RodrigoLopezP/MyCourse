using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MyCourse.Models.Entities;
using MyCourse.Models.Enums;
using MyCourse.Models.ValueTypes;

namespace MyCourse.Models.ViewModels
{
    public class CourseDetailViewModel : CourseViewModel
    {
        public string Description { get; set; }
        public List<LessonViewModel> Lessons { get; set; }
        public string RowVersion { get; set; }

        public TimeSpan TotalCourseDuration
        {
            get => TimeSpan.FromSeconds(Lessons?.Sum(l => l.Duration.TotalSeconds) ?? 0);
        }

        public static new CourseDetailViewModel FromDataRow(DataRow courseRow)
        {
            var courseDetailViewModel = new CourseDetailViewModel
            {
                Title = Convert.ToString(courseRow["Title"]),
                ImagePath = Convert.ToString(courseRow["ImagePath"]),
                Author = Convert.ToString(courseRow["Author"]),
                Rating = Convert.ToDouble(courseRow["Rating"]),

                FullPrice = new Money(
                    Enum.Parse<Currency>(Convert.ToString(courseRow["FullPrice_Currency"])),
                    Convert.ToDecimal(courseRow["FullPrice_Amount"])
                ),

                CurrentPrice = new Money(
                    Enum.Parse<Currency>(Convert.ToString(courseRow["CurrentPrice_Currency"])),
                    Convert.ToDecimal(courseRow["CurrentPrice_Amount"])
                ),

                Id = Convert.ToInt32(courseRow["Id"]),
                /*In questo caso, la variabile DESCRIPTION viene salvata perché verrà usata
                intanto viene inizializzata anche la lista Lessons, che verrà riempita dopo,
                non in questa funzione
                */
                Description = Convert.ToString(courseRow["Description"]),
                Lessons = new List<LessonViewModel>(),
                RowVersion= Convert.ToString(courseRow["RowVersion"])
            };
            return courseDetailViewModel;
        }

        public static new CourseDetailViewModel FromDataRecord(IDataRecord dataRecord)
        {
            var courseDetailViewModel = new CourseDetailViewModel
            {
                Title = Convert.ToString(dataRecord["Title"]),
                Description = Convert.ToString(dataRecord["Description"]),
                ImagePath = Convert.ToString(dataRecord["ImagePath"]),
                Author = Convert.ToString(dataRecord["Author"]),
                Rating = Convert.ToDouble(dataRecord["Rating"]),
                FullPrice = new Money(
                    Enum.Parse<Currency>(Convert.ToString(dataRecord["FullPrice_Currency"])),
                    Convert.ToDecimal(dataRecord["FullPrice_Amount"])
                ),
                CurrentPrice = new Money(
                    Enum.Parse<Currency>(Convert.ToString(dataRecord["CurrentPrice_Currency"])),
                    Convert.ToDecimal(dataRecord["CurrentPrice_Amount"])
                ),
                Id = Convert.ToInt32(dataRecord["Id"]),
                Lessons = new List<LessonViewModel>()
            };
            return courseDetailViewModel;
        }
    
     public static new CourseDetailViewModel FromEntity(Course course)
        {
            return new CourseDetailViewModel {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Author = course.Author,
                ImagePath = course.ImagePath,
                Rating = course.Rating,
                CurrentPrice = course.CurrentPrice,
                FullPrice = course.FullPrice,
                Lessons = course.Lessons
                                    .Select(lesson => LessonViewModel.FromEntity(lesson))
                                    .ToList()
            };
        }
    }
}