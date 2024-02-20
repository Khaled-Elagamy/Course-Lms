using AutoMapper;
using Course_Lms.Data.Repositories;
using Course_Lms.Logic.DTO;
using Course_Lms.Logic.Interfaces;
using Course_Lms.Models;
using Course_Lms.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Course_Lms.Logic.Services
{
	public class ChapterService : IChapterService
	{
		private readonly EfUnitOfWork database;
		public ChapterService(EfUnitOfWork uow)
		{
			database = uow;
		}
		public async Task<int> GetCourseID(int? id)
		{
			if (id == null)
				throw new ValidationException("ChapterId not set");
			var chapter = await database.Chapters.GetByIdAsync(id.Value);
			if (chapter == null)
				throw new ValidationException("Chapter not found");
			return chapter.CourseId;
		}
		public async Task<int> CreateChapter(int courseId, string Title)
		{
			var course = await database.Courses.GetByIdAsync(courseId, c => c.Chapters);
			var chapterData = new Chapter
			{
				Title = Title,
				CourseId = courseId,
			};

			if (course.Chapters.Any())
			{
				chapterData.Position = course.Chapters.Max(ch => ch.Position) + 1;
			}
			else
			{
				chapterData.Position = 1;
			}
			await database.Chapters.AddAsync(chapterData);
			return chapterData.Id;
		}
		public async Task<ChapterDTO> GetChapter(int? id)
		{
			if (id == null)
				throw new ValidationException("ChapterId not set");
			var chapter = await database.Chapters.GetByIdAsync(id.Value);
			if (chapter == null)
				throw new ValidationException("Chapter not found");
			var mapper = new MapperConfiguration(cfg => cfg.CreateMap<Chapter, ChapterDTO>()).CreateMapper();
			return mapper.Map<Chapter, ChapterDTO>(chapter);
		}
		public async Task<IEnumerable<ChapterDTO>> GetChaptersOfCourse(int? courseId)
		{
			if (courseId == null)
				throw new ValidationException("CourseId not set");
			var course = await database.Courses.GetByIdAsync(courseId.Value, c => c.Chapters);
			if (course == null)
				throw new ValidationException("Course not found");
			var sortedChapters = course.Chapters.OrderBy(chapter => chapter.Position).ToList();
			var mapper = new MapperConfiguration(cfg => cfg.CreateMap<Chapter, ChapterDTO>()).CreateMapper();
			return mapper.Map<List<ChapterDTO>>(sortedChapters);
		}
		public async Task<bool> IsChapterInCourseAsync(int chapterId, int courseId)
		{
			var chapter = await database.Chapters.GetByIdAsync(chapterId);
			return chapter != null && chapter.CourseId == courseId;
		}
		public async Task<bool> IsVideoInChapterAsync(int chapterId, string filename)
		{
			var chapter = await database.Chapters.GetByIdAsync(chapterId);
			return chapter != null && chapter.VideoUrl == filename;
		}
		public async Task UpdateChapterPropertyAsync(int chapterId, string propertyName, string newValue)
		{
			var chapter = await database.Chapters.GetByIdAsync(chapterId);

			if (chapter != null)
			{
				// Dynamically create the expression
				var parameter = Expression.Parameter(typeof(Chapter));
				var property = Expression.Property(parameter, propertyName);
				var lambda = Expression.Lambda<Func<Chapter, string>>(property, parameter);

				// Use the original method with the dynamically created expression
				await database.Chapters.UpdateChapterPropertyAsync(chapterId, lambda, newValue);
			}
			else
			{
				throw new InvalidOperationException($"Chapter with ID {chapterId} not found.");
			}
		}
		public async Task UpdateChapterIsFreeAsync(int chapterId, bool newIsFreeValue)
		{
			Expression<Func<Chapter, bool>> isFreeExpression = ch => ch.IsFree;
			await database.Chapters.UpdateChapterPropertyAsync(chapterId, isFreeExpression, newIsFreeValue);
		}

		public async Task<IEnumerable<SideBarChaptersListDTO>> SideBarChaptersList(string userId, int courseId)
		{
			var course = await database.Courses.GetByIdAsync(courseId);
			if (course == null)
				throw new ValidationException("course not found");
			bool isPurchased = await database.Purchase.HasUserPurchasedCourseAsync(userId, courseId);

			var chapters = await database.Chapters.GetPublishedChapters(courseId, userId);
			var mapper = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<Chapter, SideBarChaptersListDTO>()
				.ForMember(dest => dest.IsCompleted, opt =>
						opt.MapFrom(src => src.UserProgress != null && src.UserProgress.Count > 0 ? src.UserProgress[0].IsCompleted : (bool?)null))
					 .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => !src.IsFree && !isPurchased));
			}).CreateMapper();
			return mapper.Map<List<SideBarChaptersListDTO>>(chapters);
		}

		public async Task<bool> GetIsCompleted(int chapterId, string userId)
		{
			var chapter = await database.Chapters.GetByIdAsync(chapterId, ch => ch.UserProgress.Where(up => up.UserId == userId));
			return chapter != null && chapter.UserProgress.Any() && chapter.UserProgress[0].IsCompleted;

		}


		//Ajax calls
		public async Task<ChapterViewDTO> AutoGetChapterDetails(int chapterId, string userId)
		{
			var chapter = await database.Chapters.GetChapterDetails(chapterId, userId);
			int? nextChapterId = null;
			if (chapter == null)
				throw new ValidationException("Chapter not found");
			bool isPurchased = await database.Purchase.HasUserPurchasedCourseAsync(userId, chapter.CourseId);

			if (chapter.IsFree || isPurchased)
			{
				nextChapterId = await database.Chapters.GetNextChapter(chapter.CourseId, chapter.Position);
			}

			var mapper = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<Chapter, ChapterViewDTO>()
					.ForMember(dest => dest.IsCompleted, src => src.MapFrom(ch => ch.UserProgress.Any() && ch.UserProgress[0].IsCompleted))
					.ForMember(dest => dest.CourseId, opt => opt.MapFrom(ch => ch.Course.Id))
					.ForMember(dest => dest.Price, opt => opt.MapFrom(ch => ch.Course.Price))
					.ForMember(dest => dest.NextChapterId, opt => opt.MapFrom(_ => nextChapterId))
					.ForMember(dest => dest.IsPurchased, opt => opt.MapFrom(_ => isPurchased));

			}).CreateMapper();
			return mapper.Map<Chapter, ChapterViewDTO>(chapter);
		}

		//First call
		public async Task<ChapterViewDTO> GetChapterDetails(int courseId, string userId)
		{
			var course = await database.Courses.GetByIdAsync(courseId);
			if (course == null)
				throw new ValidationException("course not found");
			bool isPurchased = await database.Purchase.HasUserPurchasedCourseAsync(userId, courseId);
			if (isPurchased)
			{
				var chapter = await database.UserProgress.GetFirstUncompletedChapterAsync(userId, courseId);
				var mapper = new MapperConfiguration(cfg =>
				{
					cfg.CreateMap<Chapter, ChapterViewDTO>()
						.ForMember(dest => dest.IsCompleted, src => src.MapFrom(ch => ch.UserProgress.Any() && ch.UserProgress[0].IsCompleted))
						.ForMember(dest => dest.CourseId, opt => opt.MapFrom(ch => ch.Course.Id))
						.ForMember(dest => dest.Price, opt => opt.MapFrom(ch => ch.Course.Price))
						.ForMember(dest => dest.IsPurchased, opt => opt.MapFrom(_ => isPurchased));
				}).CreateMapper();
				return mapper.Map<Chapter, ChapterViewDTO>(chapter);
			}
			else
			{
				var chapter = await database.Chapters.GetFirstFreeChapterAsync(courseId);
				var mapper = new MapperConfiguration(cfg =>
				{
					cfg.CreateMap<Chapter, ChapterViewDTO>()
						.ForMember(dest => dest.IsCompleted, src => src.MapFrom(ch => ch.UserProgress.Any() && ch.UserProgress[0].IsCompleted))
						.ForMember(dest => dest.CourseId, opt => opt.MapFrom(ch => ch.Course.Id))
						.ForMember(dest => dest.Price, opt => opt.MapFrom(ch => ch.Course.Price));
				}).CreateMapper();
				return mapper.Map<Chapter, ChapterViewDTO>(chapter);
			}
		}






		public async Task TogglePublish(int chapterId, bool value)
		{
			var chapter = await database.Chapters.GetByIdAsync(chapterId, ch => ch.Course.Chapters);
			if (chapter == null)
			{
				throw new KeyNotFoundException("Chapter not found");
			}

			Expression<Func<Chapter, bool>> isPublishedExpression = ch => ch.IsPublished;
			if (string.IsNullOrEmpty(chapter.Title) || string.IsNullOrEmpty(chapter.Description) || string.IsNullOrEmpty(chapter.VideoUrl))
			{
				if (chapter.IsPublished)
				{
					await database.Chapters.UpdateChapterPropertyAsync(chapterId, isPublishedExpression, false);
				}
				throw new ArgumentException("Missing required fields");
			}
			else
			{
				await database.Chapters.UpdateChapterPropertyAsync(chapterId, isPublishedExpression, value);
				if (!value && !chapter.Course.Chapters.Any(ch => ch.IsPublished))
				{
					Expression<Func<Course, bool>> coursePublishExpression = c => c.IsPublished;
					await database.Courses.UpdateCoursePropertyAsync(chapter.CourseId, coursePublishExpression, false);
				}
			}
		}
		public async Task<CompletionViewModel> GetChapterCompletionData(int chapterId)
		{
			var (completedFields, totalFields, isPublished) = database.Chapters.CalculateCompletion(chapterId);
			var completionText = $"({completedFields}/{totalFields})";
			var isCompleted = completedFields == totalFields;
			if (isPublished && !isCompleted)
			{
				await TogglePublish(chapterId, isCompleted);
			}
			return new CompletionViewModel
			{
				CompletionText = completionText,
				IsCompleted = isCompleted,
				IsPublished = isPublished
			};
		}
		public async Task<int> DeleteChapterAsync(int chapterId)
		{
			var chapter = await database.Chapters.GetByIdAsync(chapterId, ch => ch.Course);
			if (chapter == null)
			{
				throw new KeyNotFoundException("Chapter not found");
			}
			if (chapter.VideoUrl != null)
			{
				DeleteOldVideoAsync(chapter.CourseId, chapter.VideoUrl);

			}


			await database.Chapters.DeleteChapterAsync(chapter, chapter.CourseId);
			return chapter.CourseId;
		}



		public async Task<string?> SaveVideoAsync(IFormFile file, int chapterId, string uuid, string extension)
		{
			if (file != null && file.Length > 0)
			{
				var courseId = await GetCourseID(chapterId);
				var chapter = await GetChapter(chapterId);
				var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos", $"{courseId}");

				// Ensure the directory exists
				if (!Directory.Exists(uploadsDirectory))
				{
					Directory.CreateDirectory(uploadsDirectory);
				}

				var uniqueFileName = $"{uuid}-{chapterId}.{extension}";
				var filePath = Path.Combine(uploadsDirectory, uniqueFileName);

				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					await file.CopyToAsync(fileStream);
				}
				if (chapter.VideoUrl != null)
				{
					DeleteOldVideoAsync(courseId, chapter.VideoUrl);
				}

				return uniqueFileName;
			}

			return null;
		}

		private static void DeleteOldVideoAsync(int courseId, string oldVideoUrl)
		{
			var oldVideoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos", $"{courseId}", oldVideoUrl);

			if (File.Exists(oldVideoPath))
			{
				File.Delete(oldVideoPath);
			}
		}
	}

}

