using Course_Lms.Data.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Course_Lms.Models
{
	public class Course : IEntityBase
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public string Title { get; set; }
		public string? Description { get; set; }
		public string? ImageUrl { get; set; }
		public float? Price { get; set; }
		public bool IsPublished { get; set; } = false;


		[ForeignKey("CategoryId")]
		public int? CategoryId { get; set; }
		public Category? Category { get; set; }


		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }


		public List<Chapter> Chapters { get; set; }
		public List<Purchase> Purchases { get; set; }


		public string UserId { get; set; }

	}
}
