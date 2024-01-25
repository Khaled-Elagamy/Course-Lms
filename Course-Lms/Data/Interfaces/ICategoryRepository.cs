using Course_Lms.Models;

namespace Course_Lms.Data.Interfaces
{
    public interface ICategoryRepository
    {
        IEnumerable<Category> GetAll();

    }
}
