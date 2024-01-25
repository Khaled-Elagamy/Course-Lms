using Course_Lms.Logic.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Course_Lms.ViewComponents
{
	public class VideoPlayer(IChapterService chapterService) : ViewComponent
	{
		private readonly IChapterService _chapterService = chapterService;

		public async Task<IViewComponentResult> InvokeAsync(int chapterId, string filename)
		{
			var courseId = await _chapterService.GetCourseID(chapterId);
			// Construct the video source URL using chapterId and filename
			var videoSource = $"/videos/{courseId}/{filename}";
			// Check if the video file exists
			var videoPath = Path.Combine("wwwroot", "videos", courseId.ToString(), filename);
			if (File.Exists(videoPath))
			{
				// Pass the video source to the view component
				return View("Default", videoSource);
			}
			else
			{
				return View("Default", "");
			}
		}
	}
}
