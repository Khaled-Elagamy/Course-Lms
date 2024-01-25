using System.ComponentModel.DataAnnotations;

namespace Course_Lms.ViewModels
{
	public class CreateChapter
	{
		[StringLength(50,
			MinimumLength = 1,
			ErrorMessage = "Title can be between 1 and 50 characters.")]
		public string Title { get; set; } = "NewChapter";

	}
}
