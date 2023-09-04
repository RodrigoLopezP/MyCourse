using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MyCourse.Models.InputModels;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Application.Courses
{
     public class MemoryCacheCourseService : ICachedCourseService
     {
          private readonly ICourseService _courseService;
          private readonly IMemoryCache _memCache;

          public MemoryCacheCourseService(ICourseService courseService, IMemoryCache memoryCache)
          {
               _courseService = courseService;
               _memCache = memoryCache;
          }
          public Task<List<CourseViewModel>> GetBestRatingCoursesAsync()
          {
               return _memCache.GetOrCreateAsync($"BestRatingCourses", cacheX =>
               {
                    cacheX.SetSize(1);
                    cacheX.SetAbsoluteExpiration(TimeSpan.FromSeconds(6)); //TO DO: CAMBIARE TEMPO CACHEA 60 SEC
                    return _courseService.GetBestRatingCoursesAsync();
               });
          }
          public Task<List<CourseViewModel>> GetMostRecentCoursesAsync()
          {
               return _memCache.GetOrCreateAsync($"MostRecentCourses", cacheBoh =>
               {
                    cacheBoh.SetSize(1);
                    cacheBoh.SetAbsoluteExpiration(TimeSpan.FromSeconds(6)); //TO DO:  CAMBIARE TEMPO CACHEA 60 SEC
                    return _courseService.GetMostRecentCoursesAsync();
               });
          }

          /*Sez 12 - 81 Rimuovere oggetti dalla cache e limitare uso RAM
          ToDo: ricordarsi di usare memoryCache.Remove($"Course{id}" quando aggiorni il corso,
          così non rimane to sec con i vecchi dati)
          */
          public Task<CourseDetailViewModel> GetCourseAsync(int id)
          {
               return _memCache.GetOrCreateAsync($"Course{id}", cacheEntry_nomeACaso =>
               {
                    cacheEntry_nomeACaso.SetSize(1);
                    cacheEntry_nomeACaso.SetAbsoluteExpiration(TimeSpan.FromSeconds(10));
                    return _courseService.GetCourseAsync(id);
               });
          }

          public Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel model)
          {
               //Metto in cache i risultati solo per le prime 5 pagine del catalogo, che reputo essere
               //le più visitate dagli utenti, e che perciò mi permettono di avere il maggior beneficio dalla cache.
               //E inoltre, metto in cache il risultato solo se l'utente non ha cercato nulla 
               //In questo modo riduco drasticamente il consummo di memoria RAM
               bool canCache = model.Page <= 5 && string.IsNullOrEmpty(model.Search); // se non ha cercato nulla, e si trova nelle prime 5 pagine, entra nella logica della cache.
               if (canCache)
               {
                    //Sezione 13 -91 - 
                    //Sez 13 - 88 - Implementare la funzionalità di riceca- 
                    //Rendo dinamico il nome del GetOrCreate, da "Courses" a "Course nPagina-Orderby-Ascending"
                    //Altrimenti per l'applicazione se uso la pagine courses con e senza filtro per lui è uguale, mi fa vedere le stesse info per il tempo assegnato
                    return _memCache.GetOrCreateAsync($"Courses-{model.Page}-{model.OrderBy}-{model.Ascending}", cacheEntry_ciao =>
                    {
                         cacheEntry_ciao.SetSize(1);
                         cacheEntry_ciao.SetAbsoluteExpiration(TimeSpan.FromSeconds(15));
                         return _courseService.GetCoursesAsync(model);
                    });
               }
               return _courseService.GetCoursesAsync(model);
          }

          public Task<CourseDetailViewModel> CreateCourseAsync(CourseCreateInputModel nuovoCorso)
          {
               return _courseService.CreateCourseAsync(nuovoCorso);
          }

          public Task<bool> IsTitleAvailableAsync(string title, int id)
          {
               return _courseService.IsTitleAvailableAsync(title, id);
          }

          public Task<CourseEditInputModel> GetCourseForEditingAsync(int id)
          {
               return _courseService.GetCourseForEditingAsync(id);
          }

          public async Task<CourseDetailViewModel> EditCourseAsync(CourseEditInputModel inputModel)
          {
               CourseDetailViewModel result = await _courseService.EditCourseAsync(inputModel);
               //120 - Invalidare cache della pagina del dettaglio corso
               _memCache.Remove($"Course{inputModel.Id}");
               return result;
          }

          public async Task DeleteCourseAsync(CourseDeleteInputModel inputModel)
          {
               await _courseService.DeleteCourseAsync(inputModel);
               _memCache.Remove($"Course{inputModel.Id}");
          }

          public Task SendQuestionToCourseAuthorAsync(int id, string question)
          {
               return _courseService.SendQuestionToCourseAuthorAsync(id, question);
          }

          public Task<string> GetCourseAuthorIdAsync(int courseId)
          {
               return _courseService.GetCourseAuthorIdAsync(courseId);
          }
     }
}