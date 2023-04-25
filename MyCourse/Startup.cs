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
using MyCourse.Models.Services.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MyCourse.Models.Options;

namespace MyCourse
{
    public class Startup
    {
        /*Sez-12-72-configurazione tipizzata
        *Si crea la var Configurazione di tipo IConfiguration
        *E si crea il costruttore per ottenere il i valore da appsetting.json, se esistente
        *nella solution
        */
        public IConfiguration Configuration{get;}
        public Startup(IConfiguration config){ 
            Configuration=config;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            
            // services.AddTransient<ICourseService, AdoNetCourseService>();
            services.AddTransient<IDatabaseAccessor, SqliteDatabaseAccessor>();

            //Aggiunto servizio EF a posto di Adonet, deve essere aggiunto anche il servizio di db context 
            services.AddTransient<ICourseService, AdoNetCourseService>();
            // services.AddDbContext<MyCourseDbContext>();  //usa ciclo di vita Scoped, ma registra anche un servizio di loggin, tra altre cose // sez11-lez69 RIMPIAZZATO CON addDbContextPool, per migliorare le prestazioni
            //services.AddScoped<MyCourseDbContext>();  //Metodo alternativo per indicare il servizio DbContext
            services.AddDbContextPool<MyCourseDbContext>(optionsBuilder =>
            {
                //Sez-12-72-Configurazione tipizzata
                //Questo è il modo basico di prendere un valore dal file appsettings.json               
                string connectionString =Configuration
                                            .GetSection("ConnectionStrings")
                                            .GetValue<string>("Default");
                optionsBuilder.UseSqlite(connectionString);
            });

            //Options
            /*Sez-12-72 - In questo modo passiamo la configurazione ottenuta da appsetting.json
            *a una classe di tipo "ConnectionStringsOptions", che abbiamo creato nella cartella Models
            *in questo modo prende le variabili dal file di config è tipizzato
            */
            services.Configure<ConnectionStringsOptions>(Configuration.GetSection("ConnectionStrings"));
            services.Configure<CoursesOptions>(Configuration.GetSection("Courses"));
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
