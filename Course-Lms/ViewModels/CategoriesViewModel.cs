using Course_Lms.Models;

namespace Course_Lms.ViewModels
{
	public class CategoriesViewModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string IconClassName { get; set; }
		public CategoriesViewModel(Category category)
		{
			Id = category.Id;
			Name = category.Name;
			IconClassName = GetIconClassName(category.Name);
		}
		private string GetIconClassName(string categoryName)
		{
			// Logic to determine the icon class name based on the category name
			// For simplicity, I'll use a static dictionary, but you can replace this with your actual mapping logic
			Dictionary<string, string> categoryIconMap = new Dictionary<string, string>
		{
			{ "Music", "musical-note.svg" },
			{ "Photography","camera.svg" },
			{ "Fitness", "fitness.svg" },
			{ "Computer Science", "data-science.svg" },
			{ "Engineering", "engineering.svg" }

		};

			// Use TryGetValue to handle cases where the category name is not in the map
			if (categoryIconMap.TryGetValue(categoryName, out string icon))
			{
				return icon;
			}
			else
			{
				// Set a default icon or handle the case when the category name is not in the map
				return "DefaultIcon";
			}
		}
	}
}

