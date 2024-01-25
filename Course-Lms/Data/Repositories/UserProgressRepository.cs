using Course_Lms.Data.Interfaces;
using Course_Lms.Models;
using Microsoft.EntityFrameworkCore;

namespace Course_Lms.Data.Repositories
{
	public class UserProgressRepository : IUserProgressRepository
	{

		private readonly ApplicationDbContext _context;

		public UserProgressRepository(ApplicationDbContext context)
		{
			_context = context;
		}
		public async Task<Chapter?> GetFirstUncompletedChapterAsync(string userId, int courseId)
		{
			var chapter = await _context.UserProgresses
				.Where(up => up.UserId == userId && up.Chapter.CourseId == courseId && up.Chapter.IsPublished && !up.IsCompleted)
				.Include(up => up.Chapter.Course)
				.OrderBy(up => up.Chapter.Position)
				.Select(up => up.Chapter)
				.FirstOrDefaultAsync();




			if (chapter == null)
			{
				return await _context.Courses
					.Where(c => c.Id == courseId)
					.Include(c => c.Chapters)
						.ThenInclude(ch => ch.UserProgress)
					.Select(c => c.Chapters
						.Where(ch => ch.IsPublished && !ch.UserProgress.Any(up => up.UserId == userId))
						.OrderBy(ch => ch.Position)
						.FirstOrDefault()
						?? c.Chapters
							.OrderBy(ch => ch.Position)
							.FirstOrDefault())
					.FirstOrDefaultAsync();
			}
			else
			{
				return chapter;
			}

		}
		public async Task<int> GetProgressPercentage(string userId, int courseId)
		{
			try
			{
				var publishedChapterIds = await _context.Chapters
					.Where(ch => ch.CourseId == courseId && ch.IsPublished)
					.Select(ch => ch.Id)
					.ToListAsync();

				var validCompletedChapters = await _context.UserProgresses
					.CountAsync(up => up.UserId == userId && publishedChapterIds.Contains(up.ChapterId) && up.IsCompleted);

				var progressPercentage = (double)validCompletedChapters / publishedChapterIds.Count * 100;
				var roundedProgress = (int)Math.Round(progressPercentage);

				return roundedProgress;
			}
			catch (Exception error)
			{
				Console.WriteLine("[GET_PROGRESS] " + error);
				return 0;
			}
		}
		public async Task UpsertUserProgress(string userId, int chapterId, bool isCompleted)
		{
			var existingUserProgress = await _context.UserProgresses
				.FirstOrDefaultAsync(up => up.UserId == userId && up.ChapterId == chapterId);

			if (existingUserProgress != null)
			{
				existingUserProgress.IsCompleted = isCompleted;
				_context.UserProgresses.Update(existingUserProgress);
			}
			else
			{
				var newUserProgress = new UserProgress
				{
					UserId = userId,
					ChapterId = chapterId,
					IsCompleted = isCompleted
				};
				_context.UserProgresses.Add(newUserProgress);
			}

			await _context.SaveChangesAsync();
		}
		public async Task<(bool HasFinished, int? FirstUncompletedChapterId)> CheckUserCourseCompletion(string userId, int courseId)
		{
			var chaptersInCourse = await _context.Chapters
				.Where(ch => ch.CourseId == courseId && ch.IsPublished)
				.OrderBy(ch => ch.Position)
				.ToListAsync();

			var completedChaptersForUser = await _context.UserProgresses
				 .Where(up => up.UserId == userId && chaptersInCourse.Select(ch => ch.Id).Contains(up.ChapterId) && up.IsCompleted)
				.Select(up => up.ChapterId)
				.ToListAsync();
			bool hasFinished = completedChaptersForUser.Count == chaptersInCourse.Count;

			int? firstUncompletedChapterId = null;
			if (!hasFinished)
			{
				firstUncompletedChapterId = chaptersInCourse
					.Find(ch => !completedChaptersForUser.Contains(ch.Id))?.Id;
			}

			return (hasFinished, firstUncompletedChapterId);
		}

	}
}
