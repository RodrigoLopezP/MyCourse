using System;
using System.Collections.Generic;

namespace MyCourse.Models.Entities
{
    public partial class Lesson
    {
        public Lesson(string title){
            if(String.IsNullOrEmpty(title))
            {
                throw new ArgumentException("A lesson must have a title.");
            }
        }
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TimeSpan Duration { get; set; } //00.00.00 hh.mm.ss

        public virtual Course Course { get; set; }
    }
}
