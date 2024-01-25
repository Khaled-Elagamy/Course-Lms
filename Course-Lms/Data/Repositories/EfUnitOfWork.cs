using Course_Lms.Data.Interfaces;
using Course_Lms.Models;
using Microsoft.AspNetCore.Identity;

namespace Course_Lms.Data.Repositories
{
	public class EfUnitOfWork : IUnitOfWork
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private IInstructorRepository _InstructorsRepository;
		private ICourseRepository _CoursesRepository;
		private IChapterRepository _ChaptersRepository;
		private ICategoryRepository _CategoryRepository;
		private IUserProgressRepository _UserProgressRepository;
		private IPurchaseRepository _PurchaseRepository;


		public EfUnitOfWork(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
		{
			_db = context;
			_userManager = userManager;
			_roleManager = roleManager;
			_signInManager = signInManager;
		}
		public UserManager<ApplicationUser> UserManager
		{
			get { return _userManager; }
		}
		public SignInManager<ApplicationUser> SignInManager
		{
			get { return _signInManager; }
		}

		public RoleManager<IdentityRole> RoleManager
		{
			get { return _roleManager; }
		}
		public IInstructorRepository Instructors
		{
			get
			{
				if (_InstructorsRepository == null)
					_InstructorsRepository = new InstructorRepository(_db);
				return _InstructorsRepository;
			}
		}
		public ICourseRepository Courses
		{
			get
			{
				if (_CoursesRepository == null)
					_CoursesRepository = new CourseRepository(_db);
				return _CoursesRepository;
			}
		}
		public IChapterRepository Chapters
		{
			get
			{
				if (_ChaptersRepository == null)
					_ChaptersRepository = new ChapterRepository(_db);
				return _ChaptersRepository;
			}
		}
		public ICategoryRepository Categories
		{
			get
			{
				if (_CategoryRepository == null)
					_CategoryRepository = new CategoryRepository(_db);
				return _CategoryRepository;
			}
		}
		public IUserProgressRepository UserProgress
		{
			get
			{
				if (_UserProgressRepository == null)
					_UserProgressRepository = new UserProgressRepository(_db);
				return _UserProgressRepository;
			}
		}
		public IPurchaseRepository Purchase
		{
			get
			{
				if (_PurchaseRepository == null)
					_PurchaseRepository = new PurchaseRepository(_db);
				return _PurchaseRepository;
			}
		}

		private bool disposed = false;
		public virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					_db.Dispose();
				}
				this.disposed = true;
			}
		}
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public async Task SaveAsync()
		{
			await _db.SaveChangesAsync();
		}

		public void Save()
		{

			_db.SaveChanges();
		}
	}
}
