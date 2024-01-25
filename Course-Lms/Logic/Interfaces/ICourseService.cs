using Course_Lms.Logic.DTO;
using Course_Lms.ViewModels;

namespace Course_Lms.Logic.Interfaces
{
	public interface ICourseService
	{
		Task<bool> IsTitleInDbAsync(string title);
		Task<bool> IsUserOwnerOfCourseByIdAsync(int CourseId, string userId);
		Task<int> CreateCourse(CreateCourseFirstStep course, string userId);
		Task<CourseDTO> GetCourse(int? id);
		Task<IEnumerable<CourseViewDTO>> GetCoursesBySearch(string? userId, string? title, int? categoryId);
		Task<CoursesPageViewModel> GetCourses(string? userId, string? title, int? categoryId);
		Task<int> GetCourseIdByNameAsync(string title);
		Task<IEnumerable<CourseInfoDTO>> GetInstructorCourses(string instructorid);

		Task UpdateChapterOrderAsync(int courseId, List<int> chapterOrder);
		Task UpdateCourseCategoryAsync(int courseId, string propertyName, int? newValue);

		Task UpdateCoursePriceAsync(int courseId, string propertyName, float? newValue);
		Task UpdateCoursePropertyAsync(int courseId, string propertyName, string newValue);
		Task DeleteCourseAsync(int courseId);
		Task TogglePublish(int courseId, bool value);
		Task<CompletionViewModel> GetChapterCompletionData(int courseId);
		Task<string?> SaveImageAsync(IFormFile Image, int courseId);

	}
}
