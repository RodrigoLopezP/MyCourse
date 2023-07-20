using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.Exceptions
{
    public class ImagePersistenceException:Exception
    {
        public ImagePersistenceException(Exception innerExc): base ("Errore durante la persistenza dell immagine",innerExc)
        {
            
        }
    }
}