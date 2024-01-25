using Course_Lms.Logic.DTO;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Course_Lms.ViewModels
{
	public class CourseSetupViewModel
	{
		public int Id { get; set; }
		[MaxLength(50,
			ErrorMessage = "Title can be between 1 and 50 characters.")]
		public string? Title { get; set; }
		[MaxLength(200,
			ErrorMessage = "Description can be between 1 and 200 characters.")]
		public string? Description { get; set; }
		public string? ImageUrl { get; set; }
		[Required(ErrorMessage = "Price is required.")]
		[Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
		public float? Price { get; set; }
		[Display(Name = "Course Category")]
		public int CategoryId { get; set; }
		public SelectList Categories { get; set; } = new SelectList(new List<CourseCategoryViewModel>(), nameof(CourseCategoryViewModel.Id), nameof(CourseCategoryViewModel.Name), nameof(CategoryId));
		public List<ChapterDTO>? Chapters { get; set; }
		public IFormFile? ImageFile { get; set; }

	}
}
