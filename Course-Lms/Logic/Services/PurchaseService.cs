using AutoMapper;
using Course_Lms.Data.Repositories;
using Course_Lms.Logic.DTO;
using Course_Lms.Logic.Interfaces;
using Course_Lms.Models;
using System.ComponentModel.DataAnnotations;

namespace Course_Lms.Logic.Services
{
	public class PurchaseService : IPurchaseService
	{
		private readonly EfUnitOfWork database;
		public PurchaseService(EfUnitOfWork uow)
		{
			database = uow;
		}
		public async Task RegisterNewPurchase(string userId, int courseId)
		{
			var purchaseData = new Purchase
			{
				UserId = userId,
				CourseId = courseId,
			};
			await database.Purchase.CreateAsync(purchaseData);
		}
		public async Task<CoursePaymentDTO> BeforeCheckout(string userId, int courseId)
		{
			bool isPurchased = await database.Purchase.HasUserPurchasedCourseAsync(userId, courseId);
			if (isPurchased)
			{
				throw new ValidationException("Already purchased");
			}

			var course = await database.Courses.GetByIdAsync(courseId);

			if (course == null)
			{
				throw new ValidationException("Course not found");
			}
			var mapper = new MapperConfiguration(cfg => cfg.CreateMap<Course, CoursePaymentDTO>()).CreateMapper();
			return mapper.Map<CoursePaymentDTO>(course);
		}
	}
}

