using Course_Lms.Data.Base;
using Course_Lms.Models;
using System.Linq.Expressions;

namespace Course_Lms.Data.Interfaces
{
	public interface IChapterRepository : IEntityBaseRepository<Chapter>
	{
		Task<bool> ExistsByIdAsync<TEntity>(int id, Expression<Func<TEntity, bool>> condition) where TEntity : class;

		Task UpdateChapterPropertyAsync<TProperty>(int chapterId, Expression<Func<Chapter, TProperty>> propertyExpression, TProperty newValue);
		(int completedFields, int totalFields, bool isPublished) CalculateCompletion(int chapterId);
		Task DeleteChapterAsync(Chapter chapter, int courseId);
		Task<Chapter?> GetFirstFreeChapterAsync(int courseId);
		Task<Chapter?> GetChapterDetails(int chapterId, string userId);
		Task<int?> GetNextChapter(int courseId, int? currentPosition);
		Task<List<Chapter>> GetPublishedChapters(int courseId, string userId);



	}
}
