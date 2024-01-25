using Course_Lms.Data.Base;
using Course_Lms.Data.Interfaces;
using Course_Lms.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace Course_Lms.Data.Repositories
{
	public class ChapterRepository : EntityBaseRepository<Chapter>, IChapterRepository
	{
		private readonly ApplicationDbContext _context;

		public ChapterRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}
		public async Task<bool> ExistsByIdAsync<TEntity>(int id, Expression<Func<TEntity, bool>> condition) where TEntity : class
		{
			IQueryable<TEntity> query = _context.Set<TEntity>();
			return await query.AnyAsync(condition);
		}
		public async Task UpdateChapterPropertyAsync<TProperty>(int chapterId, Expression<Func<Chapter, TProperty>> propertyExpression, TProperty newValue)
		{
			var chapter = await _context.Chapters.FindAsync(chapterId);

			var memberExpression = (MemberExpression)propertyExpression.Body;
			var propertyInfo = (PropertyInfo)memberExpression.Member;

			propertyInfo.SetValue(chapter, newValue);
			await UpdateAsync(chapterId, chapter);
		}
		public (int completedFields, int totalFields, bool isPublished) CalculateCompletion(int chapterId)
		{
			var chapter = _context.Chapters.Find(chapterId);
			int completedFields = new[] { chapter.Title, chapter.Description, chapter.VideoUrl }
				.Count(field => !string.IsNullOrEmpty(field));
			int totalFields = 3;
			bool isPublished = chapter.IsPublished;
			return (completedFields, totalFields, isPublished);
		}

		public async Task<List<Chapter>> GetPublishedChapters(int courseId, string userId)
		{
			return await _context.Chapters
			   .Where(ch => ch.CourseId == courseId && ch.IsPublished)
			   .Include(ch => ch.UserProgress.Where(up => up.UserId == userId))
				  .OrderBy(ch => ch.Position)
				.ToListAsync();
		}
		public async Task<Chapter?> GetChapterDetails(int chapterId, string userId)
		{
			return await _context.Chapters
			   .Where(ch => ch.Id == chapterId && ch.IsPublished)
			   .Include(ch => ch.UserProgress.Where(up => up.UserId == userId))
			   .Include(ch => ch.Course)
			   .FirstOrDefaultAsync();

		}


		public async Task<Chapter?> GetFirstFreeChapterAsync(int courseId)
		{
			var publishedChapters = await _context.Chapters
				.Where(ch => ch.CourseId == courseId && ch.IsPublished)
				.Include(ch => ch.Course)
				.OrderBy(ch => ch.Position)
				.ToListAsync();
			var firstFreeChapter = publishedChapters.Find(ch => ch.IsFree);

			if (firstFreeChapter != null)
			{
				return firstFreeChapter;
			}

			return publishedChapters.FirstOrDefault();
		}
		public async Task<int?> GetNextChapter(int courseId, int? currentPosition)
		{
			var chapter = await _context.Chapters
				.Where(ch => ch.CourseId == courseId && ch.IsPublished && ch.Position > currentPosition)
				.OrderBy(ch => ch.Position)
				.FirstOrDefaultAsync();
			return chapter?.Id;
		}

		public async Task DeleteChapterAsync(Chapter chapterToRemove, int courseId)
		{
			var course = _context.Courses.Include(c => c.Chapters).FirstOrDefault(c => c.Id == courseId);
			course.Chapters.Remove(chapterToRemove);
			if (course.Chapters.Count > 0 && chapterToRemove.Position == course.Chapters.Max(ch => ch.Position))
			{
				await _context.SaveChangesAsync();
			}
			else
			{
				int newPosition = 1;
				foreach (var remainingChapter in course.Chapters.OrderBy(ch => ch.Position))
				{
					remainingChapter.Position = newPosition;
					newPosition++;
				}
				await _context.SaveChangesAsync();
			}
			if (!course.Chapters.Any(ch => ch.IsPublished))
			{
				course.IsPublished = false;
				await _context.SaveChangesAsync();
			}

		}
	}
}
