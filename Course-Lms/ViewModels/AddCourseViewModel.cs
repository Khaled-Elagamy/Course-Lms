using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Course_Lms.ViewModels
{
    public class AddCourseViewModel
    {

        [Required(ErrorMessage = "Enter book title")]
        [StringLength(50,
            MinimumLength = 1,
            ErrorMessage = "Title can be between 1 and 50 characters.")]
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public SelectList Categories { get; set; } = new SelectList(new List<CourseCategoryViewModel>(), nameof(CourseCategoryViewModel.Id), nameof(CourseCategoryViewModel.Name));
        [Url] public string ImageUrl { get; set; } = null!;
        public float? Price { get; set; }

    }
}
