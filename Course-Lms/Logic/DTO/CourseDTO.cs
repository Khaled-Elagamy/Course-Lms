using Course_Lms.Models;

namespace Course_Lms.Logic.DTO
{
	public class CourseDTO
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string? Description { get; set; }
		public string? ImageUrl { get; set; }
		public float? Price { get; set; }
		public bool IsPublished { get; set; } = false;
		public string UserId { get; set; }
		public List<ChapterDTO> Chapters { get; set; }
		public List<Purchase> Purchases { get; set; }
		public int CategoryId { get; set; }
		public string? Category { get; set; }
	}
}
