using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.ViewModels
{
    public class ListViewModel<T>  //Sez13-93- Quando si dichiara il tipo nel codice, poi "Results" sarà una lista di quel tipo
    {
        public List<T> Results {get; set;}
        public int TotalCount { get; set; }
    }
}