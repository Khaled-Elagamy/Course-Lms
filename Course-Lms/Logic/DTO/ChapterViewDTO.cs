namespace Course_Lms.Logic.DTO
{
	public class ChapterViewDTO
	{
		public int Id { get; set; }
		public int CourseId { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string VideoUrl { get; set; }
		public int Position { get; set; }
		public bool IsFree { get; set; }
		public bool IsCompleted { get; set; }
		public bool IsPurchased { get; set; } = false;
		public int NextChapterId { get; set; }
		public float? Price { get; set; }
	}
}
