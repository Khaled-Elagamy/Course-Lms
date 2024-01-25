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
	public class CourseService : ICourseService
	{
		private readonly EfUnitOfWork database;
		public CourseService(EfUnitOfWork uow)
		{

			database = uow;
		}
		public async Task<CourseDTO> GetCourse(int? id)
		{
			if (id == null)
				throw new ValidationException("Course id not set");
			var course = await database.Courses.GetByIdAsync(id.Value, c => c.Chapters);
			if (course == null)
				throw new ValidationException("Course not found");
			var mapper = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<Course, CourseDTO>()
			.ForMember(dto => dto.Category, src => src.MapFrom(c => c.Category.Name))
			.ForMember(dto => dto.Chapters, opt => opt.MapFrom(src => src.Chapters));
				cfg.CreateMap<Chapter, ChapterDTO>();
			})
				.CreateMapper();
			return mapper.Map<Course, CourseDTO>(course);
		}
		public async Task<int> GetCourseIdByNameAsync(string title)
		{
			var course = await database.Courses.GetCourseByNameAsync(title);
			if (course != null)
			{
				return course.Id;
			}
			else
			{
				throw new ValidationException("Course Not Found");
			}
		}
		public async Task<IEnumerable<CourseViewDTO>> GetCoursesBySearch(string? userId, string? title, int? categoryId)
		{
			var courses = await database.Courses.GetPublishedCourses(userId, title, categoryId);
			var mapper = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<Course, CourseViewDTO>()
					.ForMember(dest => dest.CategoryName, src => src.MapFrom(c => c.Category.Name))
					.ForMember(dest => dest.NumberOfChapters, opt => opt.MapFrom(src => src.Chapters.Count))
					.ForMember(dest => dest.Progress, opt => opt.MapFrom(src => src.Purchases.Any() ? database.UserProgress.GetProgressPercentage(src.Purchases.First().UserId, src.Id).Result : (int?)null));
			})
			.CreateMapper();
			return mapper.Map<List<CourseViewDTO>>(courses);

		}

		public async Task<CoursesPageViewModel> GetCourses(string? userId, string? title, int? categoryId)
		{
			var courses = await database.Courses.GetPublishedCourses(userId, title, categoryId);

			var mapper = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<Course, CourseViewDTO>()
					.ForMember(dest => dest.CategoryName, src => src.MapFrom(c => c.Category.Name))
					.ForMember(dest => dest.NumberOfChapters, opt => opt.MapFrom(src => src.Chapters.Count))
					.ForMember(dest => dest.Progress, opt => opt.MapFrom(src => src.Purchases.Any() ? database.UserProgress.GetProgressPercentage(src.Purchases.First().UserId, src.Id).Result : (int?)null));
			})
			.CreateMapper();
			IEnumerable<Category> categories = database.Categories.GetAll();
			List<CourseViewDTO> courseDTOs = mapper.Map<List<CourseViewDTO>>(courses);
			List<CategoriesViewModel> categoriesViewModel = mapper.Map<List<CategoriesViewModel>>(categories);

			var coursesPageViewModel = new CoursesPageViewModel
			{
				Categories = categoriesViewModel,
				Courses = courseDTOs,
			};
			return coursesPageViewModel;
		}

		public async Task<IEnumerable<CourseInfoDTO>> GetInstructorCourses(string instructorid)
		{
			if (instructorid == null)
			{
				throw new ValidationException("instructorid not set");
			}
			IEnumerable<Course> courses = await database.Courses.GetCoursesByInstructorAsync(instructorid);
			var mapper = new MapperConfiguration(cfg => cfg.CreateMap<Course, CourseInfoDTO>())
			.CreateMapper();
			return mapper.Map<List<CourseInfoDTO>>(courses);
		}



		public async Task<bool> IsTitleInDbAsync(string title)
		{
			return await database.Courses.TitleExistsAsync<Course>(title);
		}

		public async Task<int> CreateCourse(CreateCourseFirstStep course, string userid)
		{
			var courseData = new Course
			{
				Title = course.Title,
				UserId = userid,
			};
			await database.Courses.AddAsync(courseData);
			return courseData.Id;
		}

		public async Task<bool> IsUserOwnerOfCourseByIdAsync(int courseId, string userId)
		{
			Expression<Func<Course, bool>> condition = c => c.Id == courseId && c.UserId == userId;
			return await database.Courses.ExistsByIdAsync(courseId, condition);
		}
		public async Task UpdateChapterOrderAsync(int courseId, List<int> chapterOrder)
		{
			var course = await database.Courses.GetByIdAsync(courseId, c => c.Chapters);
			if (course != null)
			{
				var chapterDictionary = course.Chapters.ToDictionary(c => c.Id);
				for (int i = 0; i < chapterOrder.Count; i++)
				{
					int chapterId = chapterOrder[i];

					if (chapterDictionary.TryGetValue(chapterId, out var chapter))
					{
						chapter.Position = i + 1;
					}
				}
				await database.SaveAsync();
			}
		}
		public async Task UpdateCoursePropertyAsync(int courseId, string propertyName, string newValue)
		{
			var course = await database.Courses.GetByIdAsync(courseId);

			if (course != null)
			{
				var parameter = Expression.Parameter(typeof(Course));
				var property = Expression.Property(parameter, propertyName);
				var lambda = Expression.Lambda<Func<Course, string>>(property, parameter);

				await database.Courses.UpdateCoursePropertyAsync(courseId, lambda, newValue);
			}
			else
			{
				throw new InvalidOperationException($"Course with ID {courseId} not found.");
			}
		}
		public async Task UpdateCourseCategoryAsync(int courseId, string propertyName, int? newValue)
		{
			var course = await database.Courses.GetByIdAsync(courseId);

			if (course != null)
			{
				var parameter = Expression.Parameter(typeof(Course));
				var property = Expression.Property(parameter, propertyName);
				var lambda = Expression.Lambda<Func<Course, int?>>(property, parameter);

				await database.Courses.UpdateCoursePropertyAsync(courseId, lambda, newValue);
			}
			else
			{
				throw new InvalidOperationException($"Course with ID {courseId} not found.");
			}
		}

		public async Task UpdateCoursePriceAsync(int courseId, string propertyName, float? newValue)
		{
			var course = await database.Courses.GetByIdAsync(courseId);

			if (course != null)
			{
				var parameter = Expression.Parameter(typeof(Course));
				var property = Expression.Property(parameter, propertyName);
				var lambda = Expression.Lambda<Func<Course, float?>>(property, parameter);

				await database.Courses.UpdateCoursePropertyAsync(courseId, lambda, newValue);
			}
			else
			{
				throw new InvalidOperationException($"Course with ID {courseId} not found.");
			}
		}
		public async Task DeleteCourseAsync(int courseId)
		{
			var course = await database.Courses.GetByIdAsync(courseId, c => c.Chapters);
			if (course == null)
			{
				throw new KeyNotFoundException("Course not found");
			}
			if (course.ImageUrl != null)
			{

				DeleteOldImageAsync(course.ImageUrl);
			}
			DeleteCourseFolderAsync(courseId);
			await database.Courses.DeleteCourseAsync(course);
		}
		public async Task TogglePublish(int courseId, bool value)
		{
			var course = await database.Courses.GetByIdAsync(courseId, c => c.Chapters);

			if (course == null)
			{
				throw new KeyNotFoundException("Course not found");
			}
			int completedFields = new[]
			{
				course.Title,
				course.Description,
				course.ImageUrl,
				course.Price.ToString(),
				course.CategoryId.ToString(),
			}.Count(field => !string.IsNullOrEmpty(field));
			// Check if any chapter is published
			if (course.Chapters.Any(ch => ch.IsPublished))
			{
				completedFields++;
			}
			if (completedFields < 6)
			{
				if (course.IsPublished)
				{
					Expression<Func<Course, bool>> isPublishedExpression = course => course.IsPublished;
					await database.Courses.UpdateCoursePropertyAsync(courseId, isPublishedExpression, false);
				}
				throw new ArgumentException("Missing required fields");
			}
			else
			{
				Expression<Func<Course, bool>> isPublishedExpression = course => course.IsPublished;
				await database.Courses.UpdateCoursePropertyAsync(courseId, isPublishedExpression, value);
			}
		}
		public async Task<CompletionViewModel> GetChapterCompletionData(int courseId)
		{
			var (completedFields, totalFields, isPublished) = database.Courses.CalculateCompletion(courseId);
			var completionText = $"({completedFields}/{totalFields})";
			var isCompleted = completedFields == totalFields;
			if (isPublished && !isCompleted)
			{
				await TogglePublish(courseId, isCompleted);
			}
			return new CompletionViewModel
			{
				CompletionText = completionText,
				IsCompleted = isCompleted,
				IsPublished = isPublished
			};
		}


		public async Task<string?> SaveImageAsync(IFormFile Image, int courseId)
		{
			if (Image != null && Image.Length > 0)
			{
				var course = await GetCourse(courseId);

				var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
				// Ensure the directory exists
				if (!Directory.Exists(uploadsDirectory))
				{
					Directory.CreateDirectory(uploadsDirectory);
				}

				Guid uuid = Guid.NewGuid();

				var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(Image.FileName);
				var limitedFileName = fileNameWithoutExtension.Length > 5 ? fileNameWithoutExtension.Substring(0, 5) : fileNameWithoutExtension;

				var extension = Path.GetExtension(Image.FileName);
				// Combine the limited filename and the extension
				var limitedFileNameWithExtension = limitedFileName + extension;


				var uniqueFileName = $"{uuid}_{courseId}_{limitedFileNameWithExtension}";
				var filePath = Path.Combine(uploadsDirectory, uniqueFileName);

				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					await Image.CopyToAsync(fileStream);
				}
				if (course.ImageUrl != null)
				{
					DeleteOldImageAsync(course.ImageUrl);
				}

				return uniqueFileName;
			}

			return null;
		}
		private static void DeleteCourseFolderAsync(int courseId)
		{

			var courseVideosDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos", $"{courseId}");

			// Check if the folder exists
			if (Directory.Exists(courseVideosDirectory))
			{
				// Delete the folder and its contents recursively
				Directory.Delete(courseVideosDirectory, true);
			}
		}
		private static void DeleteOldImageAsync(string oldImageUrl)
		{
			var oldVideoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", oldImageUrl);

			if (File.Exists(oldVideoPath))
			{
				File.Delete(oldVideoPath);
			}
		}

	}
}
