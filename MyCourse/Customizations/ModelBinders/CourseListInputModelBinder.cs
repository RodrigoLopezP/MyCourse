using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using MyCourse.Models.InputModels;
using MyCourse.Models.Options;

namespace MyCourse.Customizations.ModelBinders
{
    public class CourseListInputModelBinder : IModelBinder
    {
        public IOptionsMonitor<CoursesOptions> _CoursesOptions { get; }
        public CourseListInputModelBinder(IOptionsMonitor<CoursesOptions> courseOptions)
        {
            this._CoursesOptions = courseOptions;
            
        }
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            //recuperiamo i valori grazie ai value provider
            string search = bindingContext.ValueProvider.GetValue("Search").FirstValue;
            int page= Convert.ToInt32(bindingContext.ValueProvider.GetValue("Page").FirstValue);
            string orderBy=bindingContext.ValueProvider.GetValue("OrderBy").FirstValue;
            bool ascending = Convert.ToBoolean(bindingContext.ValueProvider.GetValue("Ascending").FirstValue);

            //Creiamo l'istanza del CourseListInputModel
            var inputModel= new CourseListInputModel(search,page,orderBy,ascending,_CoursesOptions.CurrentValue.PerPage, _CoursesOptions.CurrentValue.Order); 
            
            //Impostiamo il risultato per notificare che la creazione è avvenuta con successo
            bindingContext.Result=ModelBindingResult.Success(inputModel);

            //Restituiamo un task
            return Task.CompletedTask;
        }
    }
}