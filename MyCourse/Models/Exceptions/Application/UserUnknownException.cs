using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.Exceptions.Application
{
    public class UserUnknownException : Exception
    {
        public UserUnknownException() : base($"A known user is required for this operation, ahoy!")
        {
        }
    }
}