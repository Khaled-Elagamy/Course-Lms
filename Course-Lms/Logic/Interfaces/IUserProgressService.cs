namespace Course_Lms.Logic.Interfaces
{
	public interface IUserProgressService
	{
		Task<int> GetProgressOfUser(string userId, int courseId);
		Task UpdateUserProgress(string userId, int chapterId, bool isCompleted);
		Task<(bool HasFinished, int? FirstUncompletedChapterId)> CheckUserCourseCompletion(string userId, int chapterId);

	}
}
