using Course_Lms.Data.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Course_Lms.Models
{
	public class Chapter : IEntityBase
	{
		[Key]
		public int Id { get; set; }
		public string Title { get; set; }
		public string? Description { get; set; }
		public string? VideoUrl { get; set; }
		public int Position { get; set; }
		public bool IsPublished { get; set; } = false;
		public bool IsFree { get; set; } = false;

		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }

		public int CourseId { get; set; }
		[ForeignKey("CourseId")]
		public Course Course { get; set; }

		public List<UserProgress> UserProgress { get; set; }

	}
}
