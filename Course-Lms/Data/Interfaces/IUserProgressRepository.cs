using Course_Lms.Models;

namespace Course_Lms.Data.Interfaces
{
	public interface IUserProgressRepository
	{
		Task<Chapter?> GetFirstUncompletedChapterAsync(string userId, int courseId);

		Task<int> GetProgressPercentage(string userId, int courseId);
		Task UpsertUserProgress(string userId, int chapterId, bool isCompleted);
		Task<(bool HasFinished, int? FirstUncompletedChapterId)> CheckUserCourseCompletion(string userId, int courseId);


	}
}
