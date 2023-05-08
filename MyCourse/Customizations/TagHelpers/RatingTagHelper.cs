using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MyCourse.Customizations.TagHelpers
{
     public class RatingTagHelper : TagHelper
     {
          //IMPORTANTE - AGG il nome del progetto "MyCourse", per poter usare i tag helper, nel file _ViewImports
          public double Value { get; set; } // equivale al valore .rating che gli passiamo  dal cshtml, devono avere lo stesso nome altrimenti non funziona, e dovremmo usare una riga di codice più lunga per assegnarli il valore che vogliamo
          public override void Process(TagHelperContext context, TagHelperOutput output)
          {
               for (int i = 1; i <= 5; i++)
               {
                    if (Value >= i)
                    {
                         output.Content.AppendHtml("<i class=\"fas fa-star\"></i>");
                    }
                    else if (Value > i - 1)
                    {
                         output.Content.AppendHtml("<i class=\"fas fa-star-half-alt\"></i>");
                    }
                    else
                    {
                         output.Content.AppendHtml("<i class=\"far fa-star\"></i>");
                    }
               }
          }
     }
}