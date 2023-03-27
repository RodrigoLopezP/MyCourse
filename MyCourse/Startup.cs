using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using MyCourse.Models.Services.Application;

namespace MyCourse
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddTransient<ICourseService, CourseService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                lifetime.ApplicationStarted.Register(()=>
                {
                    string filePath=Path.Combine(env.ContentRootPath, "bin/reload.txt");
                    File.WriteAllText(filePath,DateTime.Now.ToString());
                });
            }
            app.UseStaticFiles();
            app.UseMvc(routeBuilder=>
            {
                // /courses/detail/5
                routeBuilder.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
                //I controllers devono essere per forza in una cartella chiamata CONTROLLERS
                //controller -> indica il file dove andrà a cercare il metodo. Es. CoursesController => nel URL ci dovrà essere scritto "Courses"
                //action -> nome del metodo
                //id -> il metodo deve avere un param di input = id . Altrimenti non riceverà niente, sarà null
            });

            // app.UseMvcWithDefaultRoute();
            // app.Run(async (context) =>
            // {
            //     if(!String.IsNullOrEmpty(context.Request.Query["nome"]))
            //     {
            //         string nome = context.Request.Query["nome"];
            //         await context.Response.WriteAsync("Hola "+nome+"!" );
            //     }
            //     else
            //     {
            //         await context.Response.WriteAsync("Test ciao ciao" );
            //     }
            // });
        }
    }
}
