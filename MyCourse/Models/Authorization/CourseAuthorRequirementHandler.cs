using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MyCourse.Models.Services.Application.Courses;

namespace MyCourse.Models.Authorization
{
     public class CourseAuthorRequirementHandler : AuthorizationHandler<CourseAuthorRequirement>
     {
          private readonly IHttpContextAccessor _httpContextAccessor;
          private readonly ICachedCourseService _courseService;
          public CourseAuthorRequirementHandler(IHttpContextAccessor httpContextAccessor, ICachedCourseService courseService)
          {
               _httpContextAccessor = httpContextAccessor;
               _courseService = courseService;

          }
          protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CourseAuthorRequirement requirement)
          {
               bool isAuthorized = false;

               //1. leggere id utente dalla sua identità
               string userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

               //2. capire quale in quale corso sta cercando di accedere
               int courseId = Convert.ToInt32(_httpContextAccessor.HttpContext.Request.RouteValues["id"]);
               if (courseId == 0)
               {
                    context.Fail();
               }

               //3. Estrarre dal db l'id dell'autore del corso selezionato
               string authorId = await _courseService.GetCourseAuthorIdAsync(courseId);

               //4. Verificare se l'id dell'utente è uguale a quello del creatore del corso
               isAuthorized = (userId == authorId);
               if (isAuthorized)
               {
                    context.Succeed(requirement);
               }
               else
               {
                    context.Fail();
               }
          }
     }
}