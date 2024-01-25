using Course_Lms.ViewModels;

namespace Course_Lms.Logic.Interfaces
{
    public interface ICategoryService
    {
        IEnumerable<CourseCategoryViewModel> GetCategories();

    }
}
