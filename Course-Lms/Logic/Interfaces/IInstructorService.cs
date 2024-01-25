using Course_Lms.ViewModels;

namespace Course_Lms.Logic.Interfaces
{
    public interface IInstructorService
    {
        Task<int> GetInstructorIdByUserIdAsync(string userId);
        Task BecomeInstructorAsync(BecomeInstructorFormModel instructor, string userId);

        Task<bool> InstructorExistsByUserIdAsync(string userId);

    }
}
