using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.Exceptions
{
    public class CourseTitleUnavailableException:Exception
    {
        public CourseTitleUnavailableException(string title, Microsoft.Data.Sqlite.SqliteException exc) : base($"Course  name unavailable")
        {
            
        }
    }
}