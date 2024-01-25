using Course_Lms.Models;
using Microsoft.AspNetCore.Identity;

namespace Course_Lms.Data.Interfaces
{
	public interface IUnitOfWork : IDisposable
	{
		UserManager<ApplicationUser> UserManager { get; }
		SignInManager<ApplicationUser> SignInManager { get; }
		RoleManager<IdentityRole> RoleManager { get; }
		ICourseRepository Courses { get; }
		IInstructorRepository Instructors { get; }
		ICategoryRepository Categories { get; }
		IChapterRepository Chapters { get; }
		void Save();
		Task SaveAsync();
	}
}
