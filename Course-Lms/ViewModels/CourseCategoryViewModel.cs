using System.ComponentModel.DataAnnotations;

namespace Course_Lms.ViewModels
{
    public class CourseCategoryViewModel
    {
        [Required] public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

    }
}
