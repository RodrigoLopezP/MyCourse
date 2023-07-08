using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using Ganss.XSS;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MyCourse.Customizations.TagHelpers
{
     [HtmlTargetElement(Attributes = "html-sanitize")] // quando un tag html avrà il html-sanitize, si triggera questo tag helper

     public class HtmlSanitizeTagHelper : TagHelper
     {
          public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
          {
               TagHelperContent tagHelperContent = await output.GetChildContentAsync(NullHtmlEncoder.Default);
               string content = tagHelperContent.GetContent(NullHtmlEncoder.Default);


               var sanitizer = CreateSanitizer();
               content = sanitizer.Sanitize(content);

               //rimettiamo il contenuto sanitizzato nel contenuto del tag
               output.Content.SetHtmlContent(content);
          }

          private static HtmlSanitizer CreateSanitizer()
          {
               var sanitizer = new HtmlSanitizer();

               //tag consentiti
               sanitizer.AllowedTags.Clear();
               sanitizer.AllowedTags.Add("b");
               sanitizer.AllowedTags.Add("i");
               sanitizer.AllowedTags.Add("p");
               sanitizer.AllowedTags.Add("br");
               sanitizer.AllowedTags.Add("ul");
               sanitizer.AllowedTags.Add("li");
               sanitizer.AllowedTags.Add("iframe");

               //attributi consentiti
               sanitizer.AllowedAttributes.Clear();
               sanitizer.AllowedAttributes.Add("src");
               sanitizer.AllowDataAttributes = false;

               //stili consentiti
               sanitizer.AllowedCssProperties.Clear();

               sanitizer.FilterUrl += FilterUrl;
               sanitizer.PostProcessNode += ProcessIFrames;

               return sanitizer;

          }

          private static void FilterUrl(object sender, FilterUrlEventArgs filterUrlEventArgs)
          {
               if (!filterUrlEventArgs.OriginalUrl.StartsWith("//www.youtube.com/") && !filterUrlEventArgs.OriginalUrl.StartsWith("https://www.youtube.com/"))
               {
                    filterUrlEventArgs.SanitizedUrl = null;
               }
          }

          private static void ProcessIFrames(object sender, PostProcessNodeEventArgs postProcessNodeEventArgs)
          {
               var iframe = postProcessNodeEventArgs.Node as IHtmlInlineFrameElement;
               if (iframe == null)
               {
                    return;
               }
               var container = postProcessNodeEventArgs.Document.CreateElement("span");
               container.ClassName = "video-container";
               container.AppendChild(iframe.Clone(true));
               postProcessNodeEventArgs.ReplacementNodes.Add(container);
          }
     }
}