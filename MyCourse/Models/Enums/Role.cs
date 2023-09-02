using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.Enums
{
    public enum Role
    {
        /*Con questo attributo verrà visualizzato correttamente nel file Pages\Admin\Users.cshtml
        dove viene usato Html.GetEnumSelectList<Role>()
        */
        [Display(Name ="Amministratore")]
        Administrator,
        [Display(Name ="Docente")]
        Teacher
    }
}