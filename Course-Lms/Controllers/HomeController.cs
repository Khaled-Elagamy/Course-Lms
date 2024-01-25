using Microsoft.AspNetCore.Mvc;

namespace Course_Lms.Controllers
{
	public class HomeController : BaseController
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error(int statusCode)
		{
			if (statusCode == 400 || statusCode == 404)
			{
				return View("Error404");
			}
			ViewData["statusCode"] = statusCode;
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

	}
}
