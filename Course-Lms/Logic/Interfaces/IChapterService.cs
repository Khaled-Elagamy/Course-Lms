using Course_Lms.Logic.DTO;
using Course_Lms.ViewModels;

namespace Course_Lms.Logic.Interfaces
{
	public interface IChapterService
	{
		Task<int> GetCourseID(int? id);
		Task<ChapterDTO> GetChapter(int? id);
		Task<IEnumerable<ChapterDTO>> GetChaptersOfCourse(int? courseId);
		Task<bool> IsChapterInCourseAsync(int chapterId, int courseId);
		Task<bool> IsVideoInChapterAsync(int chapterId, string filename);
		Task<int> CreateChapter(int courseId, string Title);
		Task UpdateChapterPropertyAsync(int chapterId, string propertyName, string newValue);
		Task UpdateChapterIsFreeAsync(int chapterId, bool newIsFreeValue);
		Task<CompletionViewModel> GetChapterCompletionData(int chapterId);
		Task TogglePublish(int chapterId, bool value);
		Task<int> DeleteChapterAsync(int chapterId);

		Task<ChapterViewDTO> GetChapterDetails(int courseId, string userId);

		Task<bool> GetIsCompleted(int chapterId, string userId);
		Task<ChapterViewDTO> AutoGetChapterDetails(int chapterId, string userId);
		Task<IEnumerable<SideBarChaptersListDTO>> SideBarChaptersList(string userId, int courseId);
		Task<string?> SaveVideoAsync(IFormFile file, int chapterId, string uuid, string extension);

	}
}
