using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using MyCourse.Models.Services.Application;
using MyCourse.Models.Services.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MyCourse.Models.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using MyCourse.Models.Enums;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace MyCourse
{
     public class Startup
     {
          /*Sez-12-72-configurazione tipizzata
          *Si crea la var Configurazione di tipo IConfiguration
          *E si crea il costruttore per ottenere il i valore da appsetting.json, se esistente
          *nella solution
          */
          public IConfiguration Configuration { get; }
          public Startup(IConfiguration config)
          {
               Configuration = config;
          }
          // This method gets called by the runtime. Use this method to add services to the container.
          // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
          public void ConfigureServices(IServiceCollection services)
          {
               services.AddResponseCaching();// Sez 12 - 84 - Response Caching
               /*Sez 12 - 83 - Aggiunta Response Cache - agg. in config file impostazione per la response cache*/
               services.AddMvc(options =>
               {
                    var homeProfile = new CacheProfile();
                    // homeProfile.Duration= Configuration.GetValue<int>("ResponseCache:Home:Duration");
                    // homeProfile.Location=Configuration.GetValue<ResponseCacheLocation>("ResponseCache:Home:Location");
                    // homeProfile.VaryByQueryKeys = new string[]{"page"}; //sez 12 - 84 questa riga di codice è superflua perché questa viene aggiunta dal .Bind sotto, è tutto settato nella config
                    Configuration.Bind("ResponseCache:Home", homeProfile);
                    options.CacheProfiles.Add("Home", homeProfile);
               }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
#if DEBUG //se nel file csproj  c'è scritto DEBUG nella riga riferita a questo package allora viene eseguito, altrimenti vuol dire che è in PROD quindi non serve
                .AddRazorRuntimeCompilation()
#endif
                ;

               Enum tipoServizioDB = Persistence.EfCore;
               switch (tipoServizioDB)
               {
                    case Persistence.Adonet:
                         services.AddTransient<ICourseService, AdoNetCourseService>();
                         services.AddTransient<IDatabaseAccessor, SqliteDatabaseAccessor>();
                         break;
                    case Persistence.EfCore:
                         services.AddTransient<ICourseService, EfCoreCourseService>();
                         services.AddDbContextPool<MyCourseDbContext>(optionsBuilder =>
                                     {
                                          //Sez-12-72-Configurazione tipizzata
                                          //Questo è il modo basico di prendere un valore dal file appsettings.json               
                                          string connectionString = Configuration
                                             .GetSection("ConnectionStrings")
                                             .GetValue<string>("Default");
                                          optionsBuilder.UseSqlite(connectionString);
                                     });
                         break;
               }

               //Aggiunto servizio EF a posto di Adonet, deve essere aggiunto anche il servizio di db context 
               // services.AddDbContext<MyCourseDbContext>();    //usa ciclo di vita Scoped, ma registra anche un servizio di loggin, tra altre cose // sez11-lez69 RIMPIAZZATO CON addDbContextPool, per migliorare le prestazioni
               //services.AddScoped<MyCourseDbContext>();        //Metodo alternativo per indicare il servizio DbContext

               //OPTIONS----------------------------------------
               /*Sez-12-72 - In questo modo passiamo la configurazione ottenuta da appsetting.json
               *a una classe di tipo "ConnectionStringsOptions", che abbiamo creato nella cartella Models
               *in questo modo prende le variabili dal file di config è tipizzato*/

               services.Configure<ConnectionStringsOptions>(Configuration.GetSection("ConnectionStrings"));
               services.Configure<CoursesOptions>(Configuration.GetSection("Courses"));
               services.Configure<KestrelServerOptions>(Configuration.GetSection("Kestrel"));
               //limitiamo la robbba direttamente dal kestrel, tipo la grandezza max di richiesta
               //https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.server.kestrel.core.kestrelserverlimits?view=aspnetcore-3.1

               /*Sez 12 - 81 Rimuovere oggetti dalla cache e limitare uso RAM*/
               services.Configure<MemoryCacheOptions>(Configuration.GetSection("MemoryCache"));

               /*Sez12 - lez79 caching
               Aggiunto servizio caching attraverso dependency injection*/
               services.AddTransient<ICachedCourseService, MemoryCacheCourseService>();

               services.AddSingleton<IImagePersister, MagickNetImagePersister>();
          }

          // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
          public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
          {
               if (env.IsEnvironment("Development"))
               {
                    app.UseDeveloperExceptionPage();

                    lifetime.ApplicationStarted.Register(() =>
                    {
                         string filePath = Path.Combine(env.ContentRootPath, "bin/reload.txt");
                         File.WriteAllText(filePath, DateTime.Now.ToString());
                    });
               }
               else
               {
                    app.UseExceptionHandler("/Error");
               }

               app.UseStaticFiles();

               //121 - nei browser nella parte EDIT non si vedevano i prezzui perchè uscivano yipi 17,99 e il browser li vuole con il punto
               //settando invariant culture per def dovrebbe uscire il punto come output e il problem è risdolto
               CultureInfo appCulture = CultureInfo.InvariantCulture;
               app.UseRequestLocalization(new RequestLocalizationOptions
               {
                    DefaultRequestCulture = new RequestCulture(appCulture),
                    SupportedCultures = new[] { appCulture }
               });
               app.UseRouting();// Sez 15 - 103 Configurato EndPoint Routing Middleware 
               app.UseResponseCaching();//Sez 12 - 84 - Response Caching

               // Sez 15 - 103  EndPoint Middleware in uso - aggiornamento a net core 3.0
               app.UseEndpoints(routeBuilder =>
               {
                    routeBuilder.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                    //controller -> indica il file dove andrà a cercare il metodo. Es. CoursesController => nel URL ci dovrà essere scritto "Courses"
                    //action -> nome del metodo
                    //id -> il metodo deve avere un param di input = id . Altrimenti non riceverà niente, sarà null
               });
          }
     }
}
