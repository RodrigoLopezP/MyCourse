using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.Exceptions
{
    public class OptimisticConcurrencyException:Exception
    {
        public OptimisticConcurrencyException():base("RowVersion cambiata, per essere modificato il corso deve essere caricato di nuovo")
        {
            
        }
    }
}