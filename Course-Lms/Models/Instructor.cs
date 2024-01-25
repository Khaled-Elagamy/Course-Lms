using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Course_Lms.Models
{
	public class Instructor
	{
		[Key] public int Id { get; set; }

		[Required]
		[MaxLength]
		public string Address { get; set; }

		[ForeignKey(nameof(UserId))]
		public string UserId { get; set; }
		public ApplicationUser User { get; set; }
	}
}
