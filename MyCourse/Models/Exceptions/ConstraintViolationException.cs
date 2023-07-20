using System;

namespace MyCourse.Models.Exceptions.Infrastructure
{
     public class ConstraintViolationException : Exception
     {
          public ConstraintViolationException(Exception innerException) : base($"excep personalizzata - A violation occurred for a database constraint", innerException)
          {
          }
     }
}