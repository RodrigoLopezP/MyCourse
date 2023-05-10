using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MyCourse
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // string ciao = args.FirstOrDefault();
            CreateWebHostBuilder(args).Build().Run();
        }

        
        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webHostBuilder => {
                    webHostBuilder.UseStartup<Startup>();
                });
    }
}
