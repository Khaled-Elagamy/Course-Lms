using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Course_Lms.Controllers
{
	public class BaseController : Controller
	{
		protected string GetUserId()
		{
			var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
			return id;
		}
	}
}
