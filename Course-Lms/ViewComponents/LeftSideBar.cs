// Ignore Spelling: Nav

using Microsoft.AspNetCore.Mvc;

namespace Course_Lms.ViewComponents
{
	public class NavLink(string controller, string action, string label, string icon)
	{
		public string Icon { get; } = icon;
		public string Label { get; } = label;
		public string Controller { get; } = controller;
		public string Action { get; } = action;
	}
	public class LeftSideBar : ViewComponent
	{
		public IViewComponentResult Invoke()
		{
			var links = new List<NavLink>();
			if (User.IsInRole("Instructor"))
			{
				links.Add(new NavLink("Instructor", "GetCourses", "My Courses", "list "));
			}
			/* will be addded
			 * else
			{
				links.Add(new NavLink("Courses", "Index", "Dashboard", "layout"));
			}*/
			links.Add(new NavLink("Courses", "Index", "Browse", "compass"));
			if (!User.IsInRole("Instructor"))
			{
				links.Add(new NavLink("Instructor", "BecomeInstructor", "BecomeInstructor", "graduation-cap"));
			}
			return View(links);
		}
	}
}