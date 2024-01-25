using AutoMapper;
using Course_Lms.Data.Repositories;
using Course_Lms.Logic.Interfaces;
using Course_Lms.Models;
using Course_Lms.ViewModels;

namespace Course_Lms.Logic.Services
{
	public class CategoryService : ICategoryService
	{
		private readonly EfUnitOfWork database;
		public CategoryService(EfUnitOfWork uow)
		{
			database = uow;
		}
		public IEnumerable<CourseCategoryViewModel> GetCategories()
		{
			var mapper = new MapperConfiguration(cfg => cfg.CreateMap<Category, CourseCategoryViewModel>()).CreateMapper();
			return mapper.Map<IEnumerable<Category>, IEnumerable<CourseCategoryViewModel>>(database.Categories.GetAll());
		}
	}
}