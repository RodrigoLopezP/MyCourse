using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyCourse.Models.InputModels;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Application.Courses
{
     public interface ICourseService
     {
          Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel model);
          Task<CourseDetailViewModel> GetCourseAsync(int id);
          Task<List<CourseViewModel>> GetBestRatingCoursesAsync();
          Task<List<CourseViewModel>> GetMostRecentCoursesAsync();
          Task<CourseDetailViewModel> CreateCourseAsync(CourseCreateInputModel nuovoCorso);
          Task<bool> IsTitleAvailableAsync(string title, int id);
          Task<CourseEditInputModel> GetCourseForEditingAsync(int id);
          Task<CourseDetailViewModel> EditCourseAsync(CourseEditInputModel inputModel);
          Task DeleteCourseAsync(CourseDeleteInputModel inputModel);
          Task SendQuestionToCourseAuthorAsync(int id, string question);
          Task<string> GetCourseAuthorIdAsync(int courseId);
          Task<int> GetCourseCountByAuthorIdAsync(string authorId);
          Task SubscribeCourseAsync(CourseSubscribeInputModel inputModel);
          Task<bool> IsCourseSubscribedAsync(int courseId, string userId);
          Task<string> GetPaymentUrlAsync(int courseId);
          Task<CourseSubscribeInputModel> CapturePaymentAsync(int id, string token);
          Task<int?> GetCourseVoteAsync(int courseId);
          Task VoteCourseAsync(CourseVoteInputModel inputModel);
     }
}