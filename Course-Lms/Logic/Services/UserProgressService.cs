using Course_Lms.Data.Repositories;
using Course_Lms.Logic.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Course_Lms.Logic.Services
{
	public class UserProgressService : IUserProgressService
	{

		private readonly EfUnitOfWork database;
		public UserProgressService(EfUnitOfWork uow)
		{
			database = uow;
		}
		public async Task<int> GetProgressOfUser(string userId, int courseId)
		{
			return await database.UserProgress.GetProgressPercentage(userId, courseId);
		}
		public async Task UpdateUserProgress(string userId, int chapterId, bool isCompleted)
		{
			var chapter = await database.Chapters.GetByIdAsync(chapterId);

			bool isPurchased = await database.Purchase.HasUserPurchasedCourseAsync(userId, chapter.CourseId);
			if (!isPurchased)
			{
				throw new ValidationException("Buy this course first");
			}
			await database.UserProgress.UpsertUserProgress(userId, chapterId, isCompleted);
		}
		public async Task<(bool HasFinished, int? FirstUncompletedChapterId)> CheckUserCourseCompletion(string userId, int chapterId)
		{
			return await database.UserProgress.CheckUserCourseCompletion(userId, chapterId);

		}
	}
}
