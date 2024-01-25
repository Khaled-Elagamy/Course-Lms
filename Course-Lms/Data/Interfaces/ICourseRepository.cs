using Course_Lms.Data.Base;
using Course_Lms.Models;
using System.Linq.Expressions;

namespace Course_Lms.Data.Interfaces
{
	public interface ICourseRepository : IEntityBaseRepository<Course>
	{
		Task<bool> TitleExistsAsync<TEntity>(string title) where TEntity : class;

		Task<bool> ExistsByIdAsync<TEntity>(int id, Expression<Func<TEntity, bool>> condition) where TEntity : class;
		Task<IEnumerable<Course>> GetCoursesByInstructorAsync(string instructorId);
		Task<Course?> GetCourseByNameAsync(string title);

		Task UpdateCoursePropertyAsync<TProperty>(int courseId, Expression<Func<Course, TProperty>> propertyExpression, TProperty newValue);
		(int completedFields, int totalFields, bool isPublished) CalculateCompletion(int courseId);
		Task DeleteCourseAsync(Course course);
		Task<List<Course>> GetPublishedCourses(string? userId, string? title = null, int? categoryId = null);

	}
}