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
        public Task<CourseDetailViewModel> GetCourseAsync(int id)
        {
            return  _memCache.GetOrCreateAsync($"Course{id}", cacheEntry_nomeACaso =>
            {
                cacheEntry_nomeACaso.SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
                return _courseService.GetCourseAsync(id);
            });
        }

        public Task<List<CourseViewModel>> GetCoursesAsync()
        {
            return _memCache.GetOrCreateAsync($"Courses",cacheEntry_ciao =>
            {
                cacheEntry_ciao.SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
                return _courseService.GetCoursesAsync();
            });
        }
    }
}