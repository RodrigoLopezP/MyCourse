using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyCourse.Models.InputModels;

namespace MyCourse.Models.ViewModels
{
    public class CourseListViewModel : IPaginationInfo
    {
        public ListViewModel<CourseViewModel> Courses { get; set; } // Models/ViewModels/ListViewModel.cs
        public CourseListInputModel Input { get; set; }    // Models/InputModels/CourseListInputModel.cs

        #region IPaginationInfo implementation
        int IPaginationInfo.CurrentPage => Input.Page;

        int IPaginationInfo.TotalResults => Courses.TotalCount;

        int IPaginationInfo.ResultsPerPage => Input.Limit;

        string IPaginationInfo.Search => Input.Search;

        string IPaginationInfo.OrderBy => Input.OrderBy;

        bool IPaginationInfo.Ascending => Input.Ascending;
        #endregion

    }
}