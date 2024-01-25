using Course_Lms.Data.Repositories;
using Course_Lms.Data.Static;
using Course_Lms.Logic.Interfaces;
using Course_Lms.Models;
using Course_Lms.ViewModels;

namespace Course_Lms.Logic.Services
{
	public class InstructorService : IInstructorService
	{
		private readonly EfUnitOfWork database;
		public InstructorService(EfUnitOfWork uow)
		{
			database = uow;
		}

		public async Task BecomeInstructorAsync(BecomeInstructorFormModel instructor, string userId)
		{
			var instructorData = new Instructor
			{
				UserId = userId,
				Address = instructor.Address,
			};
			var user = await database.UserManager.FindByIdAsync(userId);
			await database.UserManager.AddToRoleAsync(user, UserRoles.Instructor);
			await database.SignInManager.RefreshSignInAsync(user);
			await database.Instructors.CreateAsync(instructorData);
		}
		public async Task<int> GetInstructorIdByUserIdAsync(string userId)
		{
			var instructor = await database
				.Instructors
				.GetByIdAsync(userId);

			if (instructor == null)
			{
				return 0;
			}

			return instructor.Id;
		}
		public async Task<bool> InstructorExistsByUserIdAsync(string userId)
		{
			return await database
					.Instructors
					.IsFound(userId);
		}
	}
}
