using System.ComponentModel.DataAnnotations;

namespace Course_Lms.ViewModels
{
	public class BecomeInstructorFormModel
	{
		[Required]
		[Display(Name = "Address")]
		public string Address { get; set; } = null!;
	}
}
