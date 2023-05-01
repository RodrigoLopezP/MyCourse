using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Application
{
    public class MemoryCacheCourseService : ICachedCourseService
    {
        private readonly ICourseService _courseService;
        private readonly IMemoryCache _memCache;

        public MemoryCacheCourseService(ICourseService courseService, IMemoryCache memoryCache)
        {
            this._courseService = courseService;
            this._memCache = memoryCache;
        }

        /*Sez 12 - 81 Rimuovere oggetti dalla cache e limitare uso RAM
        ToDo: ricordarsi di usare memoryCache.Remove($"Course{id}" quando aggiorni il corso,
        così non rimane to sec con i vecchi dati)
        */
        public Task<CourseDetailViewModel> GetCourseAsync(int id)
        {
            return  _memCache.GetOrCreateAsync($"Course{id}", cacheEntry_nomeACaso =>
            {
                cacheEntry_nomeACaso.SetSize(1);
                cacheEntry_nomeACaso.SetAbsoluteExpiration(TimeSpan.FromSeconds(10));
                return _courseService.GetCourseAsync(id);
            });
        }

        public Task<List<CourseViewModel>> GetCoursesAsync(string search, int page, string orderBy, bool ascending)
        {
            //Sez 13 - 88 - Implementare la funzionalità di riceca- 
            //Rendo dinamico il nome del GetOrCreate,
            //Altrimenti per l'applicazione se uso la pagine courses con e senza filtro per lui è uguale, mi fa vedere le stesse info per il tempo assegnato
            return _memCache.GetOrCreateAsync($"Courses-{search}-{page}-{orderBy}-{ascending}",cacheEntry_ciao =>
            {
                cacheEntry_ciao.SetSize(1);
                cacheEntry_ciao.SetAbsoluteExpiration(TimeSpan.FromSeconds(10));
                return _courseService.GetCoursesAsync(search,page, orderBy,ascending);
            });
        }
    }
}