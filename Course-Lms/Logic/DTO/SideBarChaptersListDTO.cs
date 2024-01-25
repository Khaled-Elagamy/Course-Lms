namespace Course_Lms.Logic.DTO
{
	public class SideBarChaptersListDTO
	{
		public int Id { get; set; }
		public int CourseId { get; set; }
		public string Title { get; set; }
		public bool IsCompleted { get; set; }
		public bool IsLocked { get; set; }

	}
}
