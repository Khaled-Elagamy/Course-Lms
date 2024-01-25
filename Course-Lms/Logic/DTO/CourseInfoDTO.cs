namespace Course_Lms.Logic.DTO
{
	public class CourseInfoDTO
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public float? Price { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public bool IsPublished { get; set; }
		public string UserId { get; set; }
	}
}
