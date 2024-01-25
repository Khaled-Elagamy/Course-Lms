using Course_Lms.Data.Base;
using Course_Lms.Data.Interfaces;
using Course_Lms.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace Course_Lms.Data.Repositories
{
	public class CourseRepository : EntityBaseRepository<Course>, ICourseRepository
	{
		private readonly ApplicationDbContext _context;

		public CourseRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}
		public async Task<bool> TitleExistsAsync<TEntity>(string title) where TEntity : class
		{
			return await _context.Set<TEntity>().AnyAsync(e => EF.Property<string>(e, "Title") == title);
		}

		public async Task<bool> ExistsByIdAsync<TEntity>(int id, Expression<Func<TEntity, bool>> condition) where TEntity : class
		{
			IQueryable<TEntity> query = _context.Set<TEntity>();
			return await query.AnyAsync(condition);
		}
		public async Task UpdateCoursePropertyAsync<TProperty>(int courseId, Expression<Func<Course, TProperty>> propertyExpression, TProperty newValue)
		{
			var course = await _context.Courses.FindAsync(courseId);
			if (course != null)
			{
				var memberExpression = (MemberExpression)propertyExpression.Body;
				var propertyInfo = (PropertyInfo)memberExpression.Member;

				propertyInfo.SetValue(course, newValue);
				await UpdateAsync(courseId, course);
			}
		}
		public async Task<IEnumerable<Course>> GetCoursesByInstructorAsync(string instructorId)
		{
			return await _context.Courses.Where(c => c.UserId == instructorId).ToListAsync();
		}
		public async Task<Course?> GetCourseByNameAsync(string title)
		{
			return await _context.Courses.Where(c => c.Title == title).FirstOrDefaultAsync();
		}

		public (int completedFields, int totalFields, bool isPublished) CalculateCompletion(int courseId)
		{
			var course = _context.Courses.Include(c => c.Chapters).FirstOrDefault(c => c.Id == courseId);

			int completedFields = new[] {
				course.Title,
				course.Description,
				course.ImageUrl,
				course.Price.ToString(),
				course.CategoryId.ToString(),
			}.Count(field => !string.IsNullOrEmpty(field));


			if (course.Chapters.Any(ch => ch.IsPublished))
			{
				completedFields++;
			}
			int totalFields = 6;
			bool isPublished = course.IsPublished;
			return (completedFields, totalFields, isPublished);
		}
		public async Task DeleteCourseAsync(Course course)
		{
			_context.Courses.Remove(course);
			await _context.SaveChangesAsync();
		}

		public async Task<List<Course>> GetPublishedCourses(string? userId, string? title = null, int? categoryId = null)
		{
			var courses = await _context.Courses
			   .Where(c => c.IsPublished)
			   .Where(c => title == null || c.Title.Contains(title))
			   .Where(c => !categoryId.HasValue || c.CategoryId == categoryId.Value)
			   .Include(c => c.Category).AsNoTracking()
			   .Include(c => c.Chapters.Where(ch => ch.IsPublished)).AsNoTracking()
			   .Include(c => c.Purchases.Where(p => p.UserId == userId)).AsNoTracking()
			   .OrderByDescending(c => c.CreatedAt).ToListAsync();
			return courses;
		}

	}

}
