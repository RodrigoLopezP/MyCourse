using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MyCourse.Models.ValueTypes;

namespace MyCourse
{
     public class PriceTagHelper : TagHelper
     {
          public Money FullPrice { get; set; }
          public Money CurrentPrice { get; set; }

          public override void Process(TagHelperContext context, TagHelperOutput output)
          {
            output.TagName="span"; //il <PRICE> verrà sostituiro con <span>
            output.Content.AppendHtml($"{CurrentPrice}");
            // a quanto pare mettendo il $ davanti fa sì che il .ToString parta in automatico.
            // nella classe MONEY c'è l'override del ToString, per aggiungere sempre la valuta
            //e uno spazio prima del prezzo

            if (!CurrentPrice.Equals(FullPrice))
            {
                output.Content.AppendHtml($"<br><s>{FullPrice}</s>");
            }

          }
     }
}