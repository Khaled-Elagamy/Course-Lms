using Course_Lms.Models;

namespace Course_Lms.Data.Interfaces
{
	public interface IInstructorRepository
	{
		Task<Instructor?> GetByIdAsync(string id);
		Task<bool> IsFound(string id);
		Task CreateAsync(Instructor instructor);

	}

}
