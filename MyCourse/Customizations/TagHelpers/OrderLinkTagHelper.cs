using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MyCourse.Models.InputModels;

namespace MyCourse.Customizations.TagHelpers
{
     public class OrderLinkTagHelper : AnchorTagHelper
     {
          public string orderBy { get; set; }
          public CourseListInputModel Input { get; set; }
          public OrderLinkTagHelper(IHtmlGenerator generator) : base(generator)
          {

          }
          public override void Process(TagHelperContext context, TagHelperOutput output)
          {
               output.TagName = "a";
               RouteValues["search"] = Input.Search;
               RouteValues["orderBy"] = orderBy;
               //Se è stato cliccato lo stesso filtro di prima, allora da desc fallo diventare asc,e viceversa
               //Se è stato riordinato ma questa non è colonna selezionata, allora ASC e fine.
               RouteValues["ascending"] = (Input.OrderBy == orderBy ? !Input.Ascending : true).ToString();

               //Faccio generare l'output all'AnchorTagHelper 
               base.Process(context, output); //qui tutti i RouteValues generati sopra diventano asp-route-search e così via

               //Aggiungo l'indicatore di direzione
               if (Input.OrderBy == orderBy)
               {
                    var direc = Input.Ascending ? "up" : "down";
                    output.PostContent.SetHtmlContent($"<i class=\"fas fa-caret-{direc}\"></i>");
               }
          }
     }
}