using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Options;
using MyCourse.Models.Options;

namespace MyCourse.Models.Services.Infrastructure
{
     public class MagickNetImagePersister : IImagePersister
     {
          private readonly IOptionsMonitor<CoursesOptions> _optionsMonitor;
          private readonly IWebHostEnvironment _env;

          public MagickNetImagePersister(IOptionsMonitor<CoursesOptions> optionsMonitor, IWebHostEnvironment env)
          {
               this._optionsMonitor = optionsMonitor; //per usare la configurazione dal file json
               this._env = env;
          }
          public async Task<string> SaveCourseImageAsync(int courseId, IFormFile formFile)
          {
               //Salvare file
               string path = $"/Courses/{courseId}.jpg";//come verrà salvato nel db

               // path completo, verrà usato per indicare dove salvare di preciso il file. In questo modo usando COMBINE, funzionerà indipendente che sia Windows, MacOS, Linux, ecc
               string physicalPath = Path.Combine(_env.WebRootPath, "Courses", $"{courseId}.jpg");
               using Stream inputStream = formFile.OpenReadStream();// uso la variabile STREAM per poter manipolare l immagine
               using MagickImage image = new MagickImage(inputStream);

               //Manipolazione immagine con ImageMagick
               var resizeGeometry = new MagickGeometry(300, 300)
               {
                    FillArea = true
               };
               image.Resize(resizeGeometry);
               image.Crop(300, 300, Gravity.Center);

                image.Quality=70;
                
               //salvataggio immagine modificata su disco
               image.Write(physicalPath, MagickFormat.Jpg);

               return path;
          }
     }
}