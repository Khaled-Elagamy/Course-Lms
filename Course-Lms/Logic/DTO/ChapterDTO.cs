namespace Course_Lms.Logic.DTO
{
	public class ChapterDTO
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string? Description { get; set; }
		public string? VideoUrl { get; set; }
		public int Position { get; set; }
		public bool IsPublished { get; set; } = false;
		public bool IsFree { get; set; } = false;
		public int CourseId { get; set; }

	}
}
