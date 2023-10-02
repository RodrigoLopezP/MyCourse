using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.Customizations.ModelBinders;
using MyCourse.Models.Options;

namespace MyCourse.Models.InputModels
{
    /* Sezione 13 - 91 - Creare un ModelBinder personalizzato
        Con questa classe, nel CourseController.Index(), invece di scrivere tutte le variabili che gli arrivano lì,
        usiamo questo per raccoglierle tutte, e possiamo fare la sanitizzazione in caso di valori non voluti
        DATO che abbiamo messo tutte le varibili solo con il get, l'applicazione si incazza è dà errore
        Per questo motivo viene creato il model binder che si trova in Models/Customizations/ModelBinders/CourseListInputModelBinder.cs
        Con la riga qua sotto viene indicato che dovrà usare quel model binder personalizzato
    */
    [ModelBinder(BinderType = typeof(CourseListInputModelBinder))]
    public class CourseListInputModel
    {
        public CourseListInputModel(string search, int page, string orderBy, bool ascending,  int limit, CoursesOrderOptions coursesOptions)
        {
            var orderOptions = coursesOptions;
            if (!orderOptions.Allow.Contains(orderBy))
            {
                orderBy = orderOptions.By; //se non è arrivato dalla applicazione da dove ordinarlo, allora l'app imposta quello di default
                ascending = orderOptions.Ascending;
            }
            this.Search = search ?? "";
            this.Page = Math.Max(1, page);
            this.OrderBy = orderBy;
            this.Ascending = ascending;
            this.Limit=Math.Max(1,limit);
            Offset=(this.Page-1)*Limit;
        }
        public string Search { get; }
        public int Page { get; }
        public string OrderBy { get; }
        public bool Ascending { get; }

        public int Limit { get; }
        public int Offset { get; }

    }
}