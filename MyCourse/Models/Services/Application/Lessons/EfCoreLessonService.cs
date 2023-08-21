using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyCourse.Models.Exceptions;
using MyCourse.Models.InputModels.Lessons;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels.Lessons;

namespace MyCourse.Models.Services.Application.Lessons
{
    public class EfCoreLessonService : ILessonService
    {
        private readonly ILogger<EfCoreLessonService> logger;
        private readonly MyCourseDbContext dbContext;

        public EfCoreLessonService(ILogger<EfCoreLessonService> logger, MyCourseDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }
        public Task<LessonDetailViewModel> CreateLessonAsync(LessonCreateInputModel inputModel)
        {
            throw new NotImplementedException();
        }

        public Task<LessonDetailViewModel> EditLessonAsync(LessonEditInputModel inputModel)
        {
            throw new NotImplementedException();
        }

        public async Task<LessonDetailViewModel> GetLessonAsync(int id)
        {
            IQueryable<LessonDetailViewModel> queryLinq = dbContext.Lessons
              .AsNoTracking()
              .Where(lesson => lesson.Id == id)
              .Select(lesson => LessonDetailViewModel.FromEntity(lesson)); //Usando metodi statici come FromEntity, la query potrebbe essere inefficiente. Mantenere il mapping nella lambda oppure usare un extension method personalizzato

            LessonDetailViewModel viewModel = await queryLinq.FirstOrDefaultAsync();

            if (viewModel == null)
            {
                logger.LogWarning("Lesson {id} not found", id);
                throw new LessonNotFoundException(id);
            }

            return viewModel;
        }

        public async Task<LessonEditInputModel> GetLessonForEditingAsync(int id)
        {
            IQueryable<LessonEditInputModel> queryLinq = dbContext.Lessons
               .AsNoTracking()
               .Where(lesson => lesson.Id == id)
               .Select(lesson => LessonEditInputModel.FromEntity(lesson)); //Usando metodi statici come FromEntity, la query potrebbe essere inefficiente. Mantenere il mapping nella lambda oppure usare un extension method personalizzato

            LessonEditInputModel inputModel = await queryLinq.FirstOrDefaultAsync();

            if (inputModel == null)
            {
                logger.LogWarning("Lesson {id} not found", id);
                throw new LessonNotFoundException(id);
            }

            return inputModel;
        }
    }
}