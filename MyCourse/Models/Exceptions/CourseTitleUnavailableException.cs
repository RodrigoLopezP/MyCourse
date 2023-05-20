using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.Exceptions
{
    public class CourseTitleUnavailableException:Exception
    {
        public CourseTitleUnavailableException(string title, Exception innerExc) : base($"Course name {title} unavailable, already exists", innerExc)
        {
            
        }
    }
}