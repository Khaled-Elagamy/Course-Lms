using Course_Lms.Logic.Interfaces;
using Course_Lms.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Course_Lms.Controllers
{
	public class InstructorController : BaseController
	{
		private readonly ICourseService _courseService;
		private readonly IInstructorService _instructorService;
		public InstructorController(ICourseService courseService, IInstructorService instructorService)
		{
			_courseService = courseService;
			_instructorService = instructorService;

		}
		[Authorize]
		[HttpGet]
		public async Task<IActionResult> BecomeInstructor()
		{
			var isInstructor = await _instructorService.InstructorExistsByUserIdAsync(GetUserId());

			if (isInstructor)
			{
				TempData["ErrorMessage"] = "You are already an instructor!";
				return RedirectToAction("Index", "Courses");
			}

			return View();
		}
		[Authorize]
		[HttpPost]
		public async Task<IActionResult> BecomeInstructor(BecomeInstructorFormModel instructor)
		{
			var isInstructor = await _instructorService.InstructorExistsByUserIdAsync(GetUserId());
			if (isInstructor)
			{
				TempData["ErrorMessage"] = "You are already an instructor!";
				return RedirectToAction("Index", "Courses");
			}


			if (!ModelState.IsValid)
			{
				return View(instructor);
			}
			try
			{
				await _instructorService.BecomeInstructorAsync(instructor, GetUserId());

				TempData["SuccessMessage"] = "You are now an instructor.";
				return RedirectToAction("Index", "Courses");
			}
			catch (Exception)
			{
				return GeneralError();
			}
		}
		[Authorize]
		[HttpGet]
		[Route("/Instructor/Courses")]
		public async Task<IActionResult> GetCourses()
		{
			var instructorid = GetUserId();
			var isInstructor = await _instructorService.InstructorExistsByUserIdAsync(instructorid);
			if (!isInstructor)
			{
				TempData["ErrorMessage"] = "You must be an Instructor in order to add see Courses!";
				return RedirectToAction("Index", "Courses");
			}
			try
			{
				var courses = await _courseService.GetInstructorCourses(instructorid);
				return View("CoursesData", courses);
			}
			catch (Exception ex)
			{
				return GeneralError();
			}

		}



		private IActionResult GeneralError()
		{
			TempData["ErrorMessage"] =
				"Unexpected error occurred! Please try again later or contact administrator.";

			return RedirectToAction("Index", "Courses");
		}
	}
}
