// using Microsoft.AspNetCore.Builder; // AGGIUNGENDO in myCourse il tag ImplicitUsings enable .NET aggiunge per noi gli using implicatamente
//nella cartella obg globalUsings g cs

namespace MyCourse
{
    public class Program
    {
        public static void Main(string[] args)//magari da riga di commando vengono passati degli argomenti quando viene eseguita l'applicazione
        {
            WebApplicationBuilder builder= WebApplication.CreateBuilder(args);

            Startup startup=new (builder.Configuration);

            //Aggiungere i servizi per la dependency injection (metodo ConfigureServices)
            startup.ConfigureServices(builder.Services);

            WebApplication app= builder.Build();

            //Usiamo i middleware (metodo Configure)
            startup.Configure(app);
            app.Run();

        }
    }
}
