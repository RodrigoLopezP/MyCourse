using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MyCourse.Models.Services.Infrastructure
{
     public class InsecureImagePersister : IImagePersister
     {
          private readonly IWebHostEnvironment env;

          public InsecureImagePersister(IWebHostEnvironment env)
        {
            this.env=env;
        }
          public async Task<string> SaveCourseImageAsync(int courseId, IFormFile formFile)
          {
            string path=$"/Courses/{courseId}.jpg"; //il percorso relativo dove si troverà l img da salvare
            string physicalPath=Path.Combine(env.WebRootPath,
                                                   "Courses",
                                                   $"{courseId}.jpg");
            using FileStream fileStream= File.OpenWrite(physicalPath); // da C# 8 non si deve più aprire le parentesi tonde per indicare quando deve essere chiuso l using

            await formFile.CopyToAsync(fileStream);

            return path;
          }
     }
}