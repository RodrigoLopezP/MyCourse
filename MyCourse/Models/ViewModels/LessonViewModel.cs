using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.ViewModels
{
    public class LessonViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }   
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public static LessonViewModel FromDataRow(DataRow lessonRow)
        {
            var lessonViewModel = new LessonViewModel{
                Id= Convert.ToInt32(lessonRow["Id"]),
                Title= Convert.ToString(lessonRow["Title"]),
                Description= Convert.ToString(lessonRow["Description"]),
                Duration=TimeSpan.Parse((string)lessonRow["Duration"])
            };
            return lessonViewModel;
        }
    }
}