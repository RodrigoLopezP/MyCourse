using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Options;
using MyCourse.Models.Exceptions;
using MyCourse.Models.Options;

namespace MyCourse.Models.Services.Infrastructure
{
     public class MagickNetImagePersister : IImagePersister
     {
          private readonly IOptionsMonitor<CoursesOptions> _coursesOptions;
          private readonly IWebHostEnvironment _env;
          private readonly SemaphoreSlim semaphore;

          public MagickNetImagePersister(IOptionsMonitor<CoursesOptions> optionsMonitor, IWebHostEnvironment env)
          {
               this._coursesOptions = optionsMonitor; //per usare la configurazione dal file json
               this._env = env;

               //se la img va oltra 4000 px di altezza larghezza,  verrà sollevata una exception
               ResourceLimits.Width = 4000;
               ResourceLimits.Height = 4000;

               //server a dare un limite di stanza che posso lavorare in contemporanea,
               //per non intasare la RAM di un miliardo di persone che carica una img nello stesso istante
               semaphore = new SemaphoreSlim(2);
               ///MagickNetImagePersister è SINGLETONE nello startup così viene creata UNA sola istanza durante tutta l'esecuzione
               //così ci sarà solo questo semaphore a gestire la manipolazione di immagini
          }
          public async Task<string> SaveCourseImageAsync(int courseId, IFormFile formFile)
          {
               await semaphore.WaitAsync();//indica che una istanza verrà occupata
               try
               {
                    string path = $"/Courses/{courseId}.jpg";//come verrà salvato nel db

                    // path completo, verrà usato per indicare dove salvare di preciso il file nel server
                    //In questo modo usando COMBINE, funzionerà indipendente che sia Windows, MacOS, Linux, ecc
                    string physicalPath = Path.Combine(_env.WebRootPath, "Courses", $"{courseId}.jpg");
                    using Stream inputStream = formFile.OpenReadStream();// uso la variabile STREAM per poter manipolare l immagine
                    using MagickImage image = new MagickImage(inputStream);

                    //Manipolazione immagine con ImageMagick
                    int witdthImg = _coursesOptions.CurrentValue.Image.Width;
                    int heightImg = _coursesOptions.CurrentValue.Image.Height;
                    int qualityImg = _coursesOptions.CurrentValue.Image.Quality;
                    var resizeGeometry = new MagickGeometry(witdthImg, heightImg)
                    {
                         FillArea = true
                    };
                    image.Resize(resizeGeometry);
                    image.Crop(witdthImg, heightImg, Gravity.Center);

                    image.Quality = qualityImg;

                    //salvataggio immagine modificata su disco
                    image.Write(physicalPath, MagickFormat.Jpg);
                    return path;

               }
               catch(Exception exc){
                    throw new ImagePersistenceException(exc);
               }
               finally
               {
                    semaphore.Release(); // l istanza viene liberata, può elaborare un altra richiesta
               }
          }
     }
}