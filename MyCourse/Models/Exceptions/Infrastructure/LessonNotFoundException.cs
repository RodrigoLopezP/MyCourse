using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.Exceptions
{
    public class LessonNotFoundException : Exception
    {
        public LessonNotFoundException(int lessonId) : base($"Lesson {lessonId} not found")
        {
        }
    }
}