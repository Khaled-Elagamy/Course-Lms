using Course_Lms.Data.Interfaces;
using Course_Lms.Models;
using Microsoft.EntityFrameworkCore;

namespace Course_Lms.Data.Repositories
{
	public class PurchaseRepository : IPurchaseRepository
	{
		private readonly ApplicationDbContext _context;

		public PurchaseRepository(ApplicationDbContext context)
		{
			_context = context;
		}
		public async Task<bool> HasUserPurchasedCourseAsync(string userId, int courseId)
		{
			bool hasPurchased = await _context.Purchases
				.AnyAsync(p => p.UserId == userId && p.CourseId == courseId);
			return hasPurchased;
		}
		public async Task CreateAsync(Purchase purchaseData)
		{

			await _context.Purchases.AddAsync(purchaseData);
			await _context.SaveChangesAsync();

		}
	}
}
