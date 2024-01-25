using System.ComponentModel.DataAnnotations;

namespace Course_Lms.ViewModels
{
	public class ChapterSetupViewModel
	{

		public int Id { get; set; }
		[MaxLength(50,
			ErrorMessage = "Title can be between 1 and 50 characters.")]
		public string Title { get; set; }
		public string Description { get; set; }
		public string? VideoUrl { get; set; }
		public bool IsPublished { get; set; } = false;
		public bool IsFree { get; set; } = false;
		public int CourseId { get; set; }

	}
}
