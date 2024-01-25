namespace Course_Lms.Logic.DTO
{
	public class CourseViewDTO
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string CategoryName { get; set; }
		public string ImageUrl { get; set; }
		public int NumberOfChapters { get; set; }
		public float? Price { get; set; }
		public int? Progress { get; set; }
	}
}
