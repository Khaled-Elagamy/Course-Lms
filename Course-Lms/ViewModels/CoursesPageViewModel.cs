using Course_Lms.Logic.DTO;

namespace Course_Lms.ViewModels
{
	public class CoursesPageViewModel
	{
		public List<CategoriesViewModel> Categories { get; set; }
		public List<CourseViewDTO> Courses { get; set; }

	}
}
