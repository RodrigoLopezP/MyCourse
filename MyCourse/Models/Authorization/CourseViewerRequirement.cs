using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace MyCourse.Models.Authorization
{
    public class CourseViewerRequirement:IAuthorizationRequirement
    {
        //qui si scrivere una configurazione personalizzata per la POLICY, in questi casi però non serve
    }
}