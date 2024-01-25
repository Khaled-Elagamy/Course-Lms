using Course_Lms.Models;

namespace Course_Lms.Data.Interfaces
{
	public interface IPurchaseRepository
	{
		Task<bool> HasUserPurchasedCourseAsync(string userId, int courseId);
		Task CreateAsync(Purchase purchaseData);

	}
}
