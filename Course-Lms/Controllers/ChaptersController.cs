using AutoMapper;
using Course_Lms.Data.Static;
using Course_Lms.Logic.DTO;
using Course_Lms.Logic.Interfaces;
using Course_Lms.Models.RequestModel;
using Course_Lms.ViewModels;
using Example;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;

namespace Course_Lms.Controllers
{
	[Authorize(Roles = UserRoles.Instructor)]
	public class ChaptersController : BaseController
	{

		private readonly IChapterService _chapterService;
		private readonly ICourseService _courseService;
		private readonly IUserProgressService _progressService;
		public ChaptersController(IChapterService chapterService, ICourseService courseService, IUserProgressService progressService)
		{
			_chapterService = chapterService;
			_courseService = courseService;
			_progressService = progressService;

		}
		[AllowAnonymous]
		[HttpGet]
		[Route("Courses/{courseName:regex(^(?!index$).*$)}")]
		public IActionResult RedirectToChapterDetails(string coursename)
		{
			return RedirectToAction("ChapterDetails", new { courseName = coursename });
		}
		[AllowAnonymous]
		[HttpGet]
		[Route("Courses/{courseName}/Chapters/{chapterId?}")]
		public async Task<IActionResult> ChapterDetails(string courseName, int chapterId)
		{
			if (courseName.IsNullOrEmpty())
			{
				if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
				{
					return BadRequest("Invalid or missing course name.");
				}
				else
				{
					return RedirectToAction("Error", new { statusCode = 404 });
				}
			}

			var userid = GetUserId();
			try
			{
				int courseId = await _courseService.GetCourseIdByNameAsync(courseName);
				bool isChapterAvaliable = await _chapterService.IsChapterInCourseAsync(chapterId, courseId);
				if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
				{
					if (!isChapterAvaliable)
					{
						return BadRequest("Chapter not in this Course");
					}
					var chapterData = await _chapterService.AutoGetChapterDetails(chapterId, userid);
					ViewData["CourseName"] = courseName;
					return PartialView("~/Views/Chapters/Partials/_ChapterPartial.cshtml", chapterData);
				}

				ChapterViewDTO chapter;
				if (chapterId > 0)
				{
					if (!isChapterAvaliable)
					{
						throw new ValidationException("Chapter not in this Course");
					}
					chapter = await _chapterService.AutoGetChapterDetails(chapterId, userid);
				}
				else
				{
					chapter = await _chapterService.GetChapterDetails(courseId, userid);
				}

				if (chapter == null) { return RedirectToAction("Index", "Courses"); }
				var chaptersList = await _chapterService.SideBarChaptersList(userid, courseId);

				var chapterViewModel = new ChapterViewModel
				{
					ProgressCount = await _progressService.GetProgressOfUser(userid, courseId),
					ChaptersList = chaptersList,
					ChapterData = chapter,
				};
				ViewData["CourseName"] = courseName;
				return View(chapterViewModel);
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

		#region CreateActionRegion

		[HttpPost]
		public async Task<IActionResult> CreateChapter(int courseID, string Title)
		{
			if (!ModelState.IsValid)
			{
				return View();
			}
			try
			{
				int newChapterId = await _chapterService.CreateChapter(courseID, Title);

				var updatedChapters = await _chapterService.GetChaptersOfCourse(courseID);
				return PartialView("_ChaptersPartial", updatedChapters);

			}
			catch (Exception)
			{
				return GeneralError();
			}
		}

		#endregion

		#region UpdateActionsRegion

		[HttpGet]
		[Route("Courses/CourseSetup/{courseId}/ChapterSetup/{chapterId}")]
		public async Task<IActionResult> ChapterSetup(int courseId, int chapterId)
		{
			if (courseId < 0)
			{
				return RedirectToAction("Index", "Courses");
			}
			var isOwner = await _courseService.IsUserOwnerOfCourseByIdAsync(courseId, GetUserId());
			if (!isOwner)
			{
				TempData["ErrorMessage"] = "You are not the owner of this course!";
				return RedirectToAction("Index", "Courses");
			}
			var isChapterInCourse = await _chapterService.IsChapterInCourseAsync(chapterId, courseId);
			if (!isChapterInCourse)
			{
				TempData["ErrorMessage"] = "The specified chapter does not belong to this course!";
				return RedirectToAction("CourseSetup", "Courses", new { id = courseId });
			}

			var chapter = await _chapterService.GetChapter(chapterId);
			/*if (!chapter.IsPublished)
			{
				TempData["Warning"] = "This chapter is unpublished. It will not be visible in the course.";
			}*/

			var mapper = new MapperConfiguration(cfg => cfg.CreateMap<ChapterDTO, ChapterSetupViewModel>()).CreateMapper();
			var viewModel = mapper.Map<ChapterDTO, ChapterSetupViewModel>(chapter);
			return View(viewModel);
		}
		[HttpPost]
		public async Task<IActionResult> UpdateChapterProperty([FromBody] UpdatePropertyRequest request)
		{
			if (request != null)
			{
				try
				{
					await _chapterService.UpdateChapterPropertyAsync(request.EntityId, request.PropertyName, request.NewValue);
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
		public async Task<JsonResult> UpdateAccess(int chapterId, bool newIsFreeValue)
		{
			try
			{
				await _chapterService.UpdateChapterIsFreeAsync(chapterId, newIsFreeValue);
				return Json(new { success = true, isFreeValue = newIsFreeValue });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}
		[HttpPost]
		public async Task<ActionResult> UpdateCompletionText(int chapterId)
		{
			var completionData = await _chapterService.GetChapterCompletionData(chapterId);
			completionData.h1Text = "Chapter Creation";
			return PartialView("_CompletionTextPartial", completionData);
		}
		[HttpPost]
		public async Task<JsonResult> Publish(int chapterId)
		{
			try
			{
				await _chapterService.TogglePublish(chapterId, true);
				return Json(new { success = true, message = "Chapter Published" });

			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}
		[HttpPost]
		public async Task<JsonResult> UnPublish(int chapterId)
		{
			try
			{
				await _chapterService.TogglePublish(chapterId, false);
				return Json(new { success = true, message = "Chapter Unpublished" });

			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}
		[HttpPost]
		public async Task<ActionResult> UnCompleteChapter(int chapterId)
		{
			var userId = GetUserId();

			try
			{
				await _progressService.UpdateUserProgress(userId, chapterId, false);
				int courseId = await _chapterService.GetCourseID(chapterId);

				int progressCount = await _progressService.GetProgressOfUser(userId, courseId);
				var chaptersList = await _chapterService.SideBarChaptersList(userId, courseId);

				var sideBarItems = await this.RenderViewAsync("~/Views/Chapters/Partials/_SideBarItemsPartial.cshtml", chaptersList, true);
				var progressBar = await this.RenderViewAsync("_ProgressBarPartial", progressCount, true);

				bool chapterIsCompleted = await _chapterService.GetIsCompleted(chapterId, userId);
				var buttonState = await this.RenderViewAsync("~/Views/Chapters/Partials/_ProgressButtonsPartial.cshtml", chapterIsCompleted, true);

				return new JsonResult(new { success = true, SideBarItems = sideBarItems, ProgressBar = progressBar, Button = buttonState });

			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}

		[HttpPost]
		public async Task<ActionResult> CompleteChapter(int chapterId, int? nextChapterId, string courseName)
		{
			var userId = GetUserId();

			try
			{

				await _progressService.UpdateUserProgress(userId, chapterId, true);
				int courseId = await _courseService.GetCourseIdByNameAsync(courseName);
				int progressCount = await _progressService.GetProgressOfUser(userId, courseId);
				var chaptersList = await _chapterService.SideBarChaptersList(userId, courseId);

				var sideBarItems = await this.RenderViewAsync("~/Views/Chapters/Partials/_SideBarItemsPartial.cshtml", chaptersList, true);
				var progressBar = await this.RenderViewAsync("_ProgressBarPartial", progressCount, true);

				(bool hasFinished, int? uncompletedChapterId) = await _progressService.CheckUserCourseCompletion(userId, courseId);
				if (hasFinished)
				{
					bool chapterIsCompleted = await _chapterService.GetIsCompleted(chapterId, userId);

					var buttonState = await this.RenderViewAsync("~/Views/Chapters/Partials/_ProgressButtonsPartial.cshtml", chapterIsCompleted, true);

					return Json(new { success = true, Button = buttonState, SideBarItems = sideBarItems, ProgressBar = progressBar, message = "Congratulation you have finished the course" });
				}
				else
				{
					if (nextChapterId != null)
					{
						var chapterData = await _chapterService.AutoGetChapterDetails((int)nextChapterId, userId);
						ViewData["CourseName"] = courseName;
						var partialViewHtml = await this.RenderViewAsync("~/Views/Chapters/Partials/_ChapterPartial.cshtml", chapterData, true);

						return Json(new { NextChapterView = partialViewHtml, SideBarItems = sideBarItems, ProgressBar = progressBar });
					}
					else
					{

						var chapterData = await _chapterService.AutoGetChapterDetails((int)uncompletedChapterId, userId);
						ViewData["CourseName"] = courseName;
						var partialViewHtml = await this.RenderViewAsync("~/Views/Chapters/Partials/_ChapterPartial.cshtml", chapterData, true);

						return Json(new { NextChapterView = partialViewHtml, SideBarItems = sideBarItems, ProgressBar = progressBar });

					}
				}
			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}
		[HttpPost]
		public async Task<JsonResult> DeleteChapter(int chapterId)
		{
			try
			{
				int courseId = await _chapterService.DeleteChapterAsync(chapterId);
				var successResult = new { success = true, message = "Chapter deleted successfully", redirectUrl = Url.Action("CourseSetup", "Courses", new { id = courseId }) };

				return Json(successResult);
			}
			catch
			{
				return Json(new { success = false, message = "Chapter not found" });

			}

		}

		#endregion

		#region VideoActionsRegion

		[HttpGet]
		public async Task<IActionResult> VideoPlayer(int chapterId, string filename)
		{
			var isFound = await _chapterService.IsVideoInChapterAsync(chapterId, filename);
			if (isFound)
			{
				return ViewComponent("VideoPlayer", new { chapterId, filename });
			}
			return Json(new { message = "error playing the video" });
		}
		[HttpPost]
		public async Task<IActionResult> UploadVideo()
		{
			try
			{
				var file = Request.Form.Files[0];
				if (file != null && file.Length > 0)
				{
					if (int.TryParse(Request.Form["chapterId"], out int chapterId))
					{
						var uuid = Request.Form["uuid"];
						var extension = Request.Form["extension"];

						var uniqueFileName = await _chapterService.SaveVideoAsync(file, chapterId, uuid, extension);

						if (!string.IsNullOrEmpty(uniqueFileName))
						{
							await _chapterService.UpdateChapterPropertyAsync(chapterId, "VideoUrl", uniqueFileName);
							return Json(new { Message = "File uploaded successfully", filePath = uniqueFileName });
						}
						else
						{
							return BadRequest(new { Message = "File upload failed" });
						}
					}
					else
					{
						return BadRequest(new { success = false, Message = "Invalid chapterId" });
					}
				}
				return BadRequest(new { Message = "File upload failed" });
			}
			catch (Exception ex)
			{
				// Handle exceptions (optional)
				return BadRequest(new { Message = "An error occurred: " + ex.Message });
			}
		}

		#endregion

		private IActionResult GeneralError()
		{
			TempData["ErrorMessage"] =
				"Unexpected error occurred! Please try again later or contact administrator.";

			return RedirectToAction("Index", "Courses");
		}
	}
}