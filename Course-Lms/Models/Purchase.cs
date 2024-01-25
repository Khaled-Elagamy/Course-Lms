using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Course_Lms.Models
{
	[Index(nameof(UserId), nameof(CourseId), IsUnique = true)]
	public class Purchase
	{
		[Key]
		public int Id { get; set; }

		public string UserId { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public DateTime UpdatedAt { get; set; }

		public int CourseId { get; set; }
		[ForeignKey("CourseId")]
		public Course Course { get; set; }

	}
}
