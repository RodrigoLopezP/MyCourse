using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.Services.Infrastructure
{
     public class SqliteDatabaseAccessor : IDatabaseAccessor
     {
          public DataSet Query(string query)
          {
               throw new NotImplementedException();
          }
     }
}