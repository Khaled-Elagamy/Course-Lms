using Course_Lms.Logic.DTO;

namespace Course_Lms.Logic.Interfaces
{
	public interface IPurchaseService
	{
		Task<CoursePaymentDTO> BeforeCheckout(string userId, int courseId);
		Task RegisterNewPurchase(string userId, int courseId);

	}
}
