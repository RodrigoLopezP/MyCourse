using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.Exceptions
{
     public class CourseImageInvalidException : Exception
     {
          public CourseImageInvalidException(int courseId, Exception exc) : base($"Course {courseId} image is not valid", exc)
          {

          }
     }
}