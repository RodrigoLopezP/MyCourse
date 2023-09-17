using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using AngleSharp.Services;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using MyCourse.Controllers;
using MyCourse.Models.Entities;
using MyCourse.Models.Services.Application.Courses;
using MyCourse.Models.Services.Application.Lessons;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels;
using MyCourse.Models.ViewModels.Lessons;

namespace MyCourse.Models.Services.Worker
{
    /*Questo è un HOSTED SERVICE*/

    public class UserDataHostedService : BackgroundService, IUserDataService
    {
        private readonly BufferBlock<string> queue = new();
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;
        private readonly LinkGenerator linkGenerator;
        private readonly IHostEnvironment env;
        private readonly IEmailClient emailClient;
        private readonly IServer server;
        public UserDataHostedService(ILogger<UserDataHostedService> logger,
                                    IServiceProvider serviceProvider,
                                    LinkGenerator linkGenerator,
                                    IHostEnvironment env,
                                    IServer server,
                                    IEmailClient emailClient)
        {
            this.serviceProvider = serviceProvider;
            this.linkGenerator = linkGenerator;
            this.env = env;
            this.emailClient = emailClient;
            this.logger = logger;
            this.server = server;
        }
        public void EnqueueUserDataDownload(string userId)
        {
            queue.Post(userId);
        }

        public IEnumerable<string> EnumerateAllUserDataZipFileLocations()
        {
               string zipRootDirectoryPath = GetZipRootDirectoryPath();
               return Directory.EnumerateFiles(zipRootDirectoryPath, "*.zip");
        }

        public string GetUserDataZipFileLocation(string userId, Guid zipFileId)
        {
            string zipFileName = $"{userId}_{zipFileId}.zip";
            string zipRootDirectoryPath = GetZipRootDirectoryPath();
            string zipFilePath = Path.Combine(zipRootDirectoryPath, zipFileName);
            return zipFilePath;
        }

        private string GetZipRootDirectoryPath()
        {
            return Path.Combine(env.ContentRootPath, "Downloads");
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
               while (!stoppingToken.IsCancellationRequested)
               {
                    string userId = null;
                    try
                    {
                         userId = await queue.ReceiveAsync(stoppingToken);

                         using (IServiceScope serviceScope = serviceProvider.CreateScope())
                         {
                              IServiceProvider serviceProvider = serviceScope.ServiceProvider;
                              ICourseService courseService = serviceProvider.GetRequiredService<ICourseService>();
                              ILessonService lessonService = serviceProvider.GetRequiredService<ILessonService>();
                              UserManager<ApplicationUser> userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                              ApplicationUser user = await userManager.FindByIdAsync(userId);

                              string zipFileUrl = await CreateZipFileAsync(userId, courseService, lessonService, stoppingToken);
                              await SendZipFileLinkToUserAsync(user.Email, zipFileUrl, stoppingToken);
                         }
                    }
                    catch (Exception exc)
                    {
                         if (!stoppingToken.IsCancellationRequested)
                         {
                              logger.LogError(exc, "Error while preparing data for user {userId}", userId);
                         }
                    }
               }
        }
        private async Task<string> CreateZipFileAsync(string userId, ICourseService courseService, ILessonService lessonService , CancellationToken stoppingToken)
        {
         stoppingToken.ThrowIfCancellationRequested();

        Guid zipFileId = Guid.NewGuid();
        string zipFilePath = GetUserDataZipFileLocation(userId, zipFileId);

        using FileStream file = File.OpenWrite(zipFilePath);
        using ZipArchive zip = new(file, ZipArchiveMode.Create);
        
        List<CourseViewModel> courses = await courseService.GetCoursesByAuthorAsync(userId);

        foreach (CourseViewModel course in courses)
        {
            CourseDetailViewModel courseDetail = await courseService.GetCourseAsync(course.Id);
            await AddZipEntry(zip, $"Corsi/{course.Id}/Descrizione.txt", $"{course.Title}\r\n{courseDetail.Description}", stoppingToken);
            
            using FileStream imageStream = File.OpenRead(Path.Combine(env.ContentRootPath, "wwwroot", "Courses", $"{courseDetail.Id}.jpg"));
            await AddZipEntry(zip, $"Corsi/{course.Id}/Image.jpg", imageStream, stoppingToken);

            foreach (LessonViewModel lesson in courseDetail.Lessons)
            {
                LessonDetailViewModel lessonDetail = await lessonService.GetLessonAsync(lesson.Id);
                await AddZipEntry(zip, $"Corsi/{course.Id}/Lezioni/{lessonDetail.Id}.txt", $"{lessonDetail.Title}\r\n{lessonDetail.Description}", stoppingToken);
            }
        }

        IServerAddressesFeature feature = server.Features.Get<IServerAddressesFeature>();
        Uri serverUri = new Uri(feature.Addresses.First());

        string zipDownloadUrl = linkGenerator.GetUriByAction(action: nameof(UserDataController.Download), controller: "UserData", values: new { id = zipFileId }, scheme: serverUri.Scheme, host: new HostString(serverUri.Host, serverUri.Port));
        return zipDownloadUrl;
        }
        private async Task AddZipEntry(ZipArchive zip, string entryName, string entryContent, CancellationToken stoppingToken)
        {
            ZipArchiveEntry entry = zip.CreateEntry(entryName, CompressionLevel.NoCompression);// nessuna compressione per evitare che l'attività occupi troppa ram 
            using Stream entryStream = entry.Open();
            using StreamWriter streamWriter = new(entryStream);
            await streamWriter.WriteAsync(entryContent);
        }
          private async Task AddZipEntry(ZipArchive zip, string entryName, Stream entryContent, CancellationToken stoppingToken)
          {
               ZipArchiveEntry entry = zip.CreateEntry(entryName, CompressionLevel.NoCompression);
               using Stream entryStream = entry.Open();
               await entryContent.CopyToAsync(entryStream);
          }
        private async Task EmailZipFileLinkToUser(ApplicationUser user, string zipFileUrl, CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            await emailClient.SendEmailAsync(user.Email, "I tuoi corsi", $"Il file zip contenente i dati dei corsi video è pronto. Lo puoi scaricare da <a href=\"{zipFileUrl}\">{zipFileUrl}</a>");
        }

          private async Task SendZipFileLinkToUserAsync(string userEmail, string zipFileUrl, CancellationToken stoppingToken)
          {
               stoppingToken.ThrowIfCancellationRequested();
               await emailClient.SendEmailAsync(userEmail, null, "I tuoi corsi", $"Il file zip contenente i dati dei corsi video è pronto. Lo puoi scaricare da <a href=\"{zipFileUrl}\">{zipFileUrl}</a>", stoppingToken);
          }
    }
}