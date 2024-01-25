using AutoMapper;
using Course_Lms.Data.Static;
using Course_Lms.Logic.DTO;
using Course_Lms.Logic.Interfaces;
using Course_Lms.Models.RequestModel;
using Course_Lms.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Course_Lms.Controllers
{
	[Authorize(Roles = UserRoles.Instructor)]
	public class CoursesController : BaseController
	{
		private readonly ICourseService _courseService;
		private readonly IChapterService _chapterService;
		private readonly IInstructorService _instructorService;
		private readonly ICategoryService _categoryService;
		public CoursesController(ICourseService courseService, IChapterService chapterService, IInstructorService instructorService, ICategoryService categoryService)
		{
			_courseService = courseService;
			_chapterService = chapterService;
			_instructorService = instructorService;
			_categoryService = categoryService;

		}
		[AllowAnonymous]
		[HttpGet]
		public async Task<IActionResult> Index(int? categoryId, string? title)
		{
			var userid = GetUserId();
			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				var CoursesData = await _courseService.GetCoursesBySearch(userid, title, categoryId);
				return PartialView("~/Views/Courses/Partials/_CourseCardPartial.cshtml", CoursesData);

			}
			var CoursesViewModel = await _courseService.GetCourses(userid, title, categoryId);
			return View(CoursesViewModel);
		}


		#region CreateActionsRegion

		[HttpGet]
		[Route("Courses/New/CreateCourse")]
		public async Task<IActionResult> Create()
		{
			var isInstructor = await _instructorService.InstructorExistsByUserIdAsync(GetUserId());
			if (!isInstructor)
			{
				TempData["ErrorMessage"] = "You must be an Instructor in order to add new Courses!";
				return RedirectToAction("Index", "Courses");
			}
			return View();
		}
		[HttpPost]
		[Route("Courses/Api/CreateCourse")]
		public async Task<IActionResult> Create(CreateCourseFirstStep viewModel)
		{
			if (!ModelState.IsValid)
			{
				return View(viewModel);
			}
			var instructorId = await _instructorService.GetInstructorIdByUserIdAsync(GetUserId());
			if (instructorId == 0)
			{
				TempData["ErrorMessage"] = "You must be an Instructor in order to add new Courses!";
				return RedirectToAction("Index", "Courses");
			}
			try
			{
				int newCourseId = await _courseService.CreateCourse(viewModel, GetUserId());
				return RedirectToAction("CourseSetup", new { id = newCourseId });
			}
			catch (Exception)
			{
				return GeneralError();
			}
		}

		[HttpPost]
		[Route("Courses/Api/CheckTitleExistence")]
		public async Task<IActionResult> CheckTitleExistence(string title)
		{
			bool exists = await _courseService.IsTitleInDbAsync(title);
			return Json(new { exists = exists });
		}

		#endregion

		#region UpdateActionsRegion

		[HttpGet]
		[Route("Courses/CourseSetup/{id}")]
		public async Task<IActionResult> CourseSetup(int id)
		{
			if (id < 0)
			{
				return RedirectToAction("Index", "Courses");
			}
			try
			{
				var course = await _courseService.GetCourse(id);
				var isOwner = await _courseService.IsUserOwnerOfCourseByIdAsync(id, GetUserId());
				if (!isOwner)
				{
					TempData["ErrorMessage"] = "You are not the owner of this course!";
					return RedirectToAction("Index", "Courses");
				}

				var mapper = new MapperConfiguration(cfg => cfg.CreateMap<CourseDTO, CourseSetupViewModel>()).CreateMapper();
				var viewModel = mapper.Map<CourseDTO, CourseSetupViewModel>(course);
				viewModel.Categories = new SelectList(_categoryService.GetCategories(), "Id", "Name", course.CategoryId);
				return View(viewModel);
			}
			catch (Exception ex)
			{
				if (ex is ValidationException)
				{

					TempData["ErrorMessage"] = ex.Message;
					return RedirectToAction("Index", "Courses");
				}
				else
				{
					return GeneralError();
				}
			}
		}
		[HttpGet]
		[Route("Courses/Api/GetChaptersData")]
		public async Task<IActionResult> GetChaptersData(int courseId)
		{
			try
			{
				var chapters = await _chapterService.GetChaptersOfCourse(courseId);
				return PartialView("_ChaptersPartial", chapters);
			}
			catch (Exception)
			{
				return GeneralError();
			}
		}
		[HttpPost]
		[Route("Courses/Api/UpdateCourseProperty")]

		public async Task<IActionResult> UpdateCourseProperty([FromBody] UpdatePropertyRequest request)
		{
			if (request != null)
			{
				try
				{
					if (request.PropertyName == "Title")
					{
						bool exists = await _courseService.IsTitleInDbAsync(request.NewValue);
						if (exists)
						{
							return Json(new { success = false, exists = exists });
						}
					}
					if (request.PropertyName == "Price")
					{
						float? floatValue = float.TryParse(request.NewValue, out float parsedValue) ? parsedValue : (float?)null;

						if (floatValue.HasValue)
						{
							await _courseService.UpdateCoursePriceAsync(request.EntityId, request.PropertyName, floatValue);
						}
						else
						{
							return BadRequest("Invalid float representation");
						}
					}
					else if (request.PropertyName == "CategoryId")
					{
						int? categoryId = int.TryParse(request.NewValue, out int intValue) ? intValue : (int?)null;

						if (categoryId.HasValue)
						{
							await _courseService.UpdateCourseCategoryAsync(request.EntityId, request.PropertyName, categoryId);
						}
						else
						{
							return BadRequest("Invalid int representation");
						}
					}
					else
					{
						// Assuming courseId, propertyExpression, and newValue are part of the request
						await _courseService.UpdateCoursePropertyAsync(request.EntityId, request.PropertyName, request.NewValue);
					}

					return Json(new { success = true });
				}
				catch (Exception ex)
				{
					return Json(new { success = false, error = ex.Message });
				}
			}
			return Json(new { success = false, error = "Invalid request data" });
		}
		[HttpPost]
		[Route("Courses/Api/UpdateChapterOrder")]

		public async Task<IActionResult> UpdateChapterOrder(int courseId, List<int> chapterOrder)
		{
			if (chapterOrder == null || chapterOrder.Count == 0)
			{
				return Json(new { success = false, message = "Chapter order data is missing." });
			}
			try
			{
				await _courseService.UpdateChapterOrderAsync(courseId, chapterOrder);
				return Json(new { success = true });

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return StatusCode(500, new { success = false, message = "An error occurred while updating chapter order." });
			}
		}
		[HttpPost]
		[Route("Courses/Api/UploadImage")]

		public async Task<IActionResult> UploadImage(int CourseId, IFormFile ImageFile)
		{
			try
			{
				if (ImageFile != null && ImageFile.Length > 0)
				{
					var uniqueFileName = await _courseService.SaveImageAsync(ImageFile, CourseId);

					if (!string.IsNullOrEmpty(uniqueFileName))
					{
						await _courseService.UpdateCoursePropertyAsync(CourseId, "ImageUrl", uniqueFileName);
						return Json(new { success = true, Message = "Image uploaded successfully", image = uniqueFileName });
					}
					else
					{
						return BadRequest(new { success = true, Message = "Image upload failed" });
					}
				}
				// If the image is not provided, handle the error (e.g., show an error message)
				ModelState.AddModelError("ImageFile", "Please choose a file to upload.");
				return Json(new { success = false, error = "Invalid request data" });
			}
			catch (Exception ex)
			{
				// Handle exceptions (optional)
				return BadRequest(new { Message = "An error occurred: " + ex.Message });
			}
		}
		[HttpPost]
		[Route("Courses/Api/UpdateCompletionText")]

		public async Task<ActionResult> UpdateCompletionText(int courseId)
		{
			var completionData = await _courseService.GetChapterCompletionData(courseId);
			completionData.h1Text = "Course Setup";
			return PartialView("_CompletionTextPartial", completionData);
		}
		[HttpPost]
		[Route("Courses/Api/Publish")]

		public async Task<IActionResult> Publish(int courseId)
		{
			try
			{
				await _courseService.TogglePublish(courseId, true);
				return Json(new { success = true, message = "Course Published" });

			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}
		[HttpPost]
		[Route("Courses/Api/UnPublish")]

		public async Task<IActionResult> UnPublish(int courseId)
		{
			try
			{
				await _courseService.TogglePublish(courseId, false);
				return Json(new { success = true, message = "Course Unpublished" });

			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}
		[HttpPost]
		[Route("Courses/Api/DeleteCourse")]

		public async Task<IActionResult> DeleteCourse(int courseId)
		{
			try
			{
				await _courseService.DeleteCourseAsync(courseId);
				var successResult = new { success = true, message = "Course deleted successfully", redirectUrl = Url.Action("GetCourses", "Instructor") };

				return Json(successResult);
			}
			catch
			{
				return Json(new { success = false, message = "Course not found" });

			}

		}

		#endregion

		[HttpGet]
		[Route("Courses/Api/CancelAction")]

		public IActionResult CancelAction()
		{
			return RedirectToAction("Index");
		}
		private IActionResult GeneralError()
		{
			TempData["ErrorMessage"] =
				"Unexpected error occurred! Please try again later or contact administrator.";

			return RedirectToAction("Index", "Courses");
		}
	}
}
