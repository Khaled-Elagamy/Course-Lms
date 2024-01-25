using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Course_Lms.Models
{
	[Index(nameof(UserId), nameof(ChapterId), IsUnique = true)]

	public class UserProgress
	{
		[Key]
		public int Id { get; set; }
		public bool IsCompleted { get; set; } = false;

		public string UserId { get; set; }


		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public DateTime UpdatedAt { get; set; }


		[ForeignKey("ChapterId")]
		public int ChapterId { get; set; }
		public Chapter Chapter { get; set; }
	}
}
