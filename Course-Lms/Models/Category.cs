using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Course_Lms.Models
{
	[Index(nameof(Name), IsUnique = true)]
	public class Category
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[MaxLength]
		public string Name { get; set; }
		public List<Course> Courses { get; set; }
	}
}
