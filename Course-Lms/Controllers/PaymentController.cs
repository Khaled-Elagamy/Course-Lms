using Course_Lms.Logic.Interfaces;
using Course_Lms.Models.StripeSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System.ComponentModel.DataAnnotations;

namespace Course_Lms.Controllers
{
	public class PaymentController : BaseController
	{
		private readonly IPurchaseService _purchaseService;

		public string SessionId { get; set; }
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly StripeSettings _stripeSettings;

		public PaymentController(IPurchaseService purchaseService, IHttpContextAccessor httpContextAccessor, IOptions<StripeSettings> stripeSettings)
		{
			_httpContextAccessor = httpContextAccessor;
			_stripeSettings = stripeSettings.Value;
			_purchaseService = purchaseService;
		}
		public async Task<IActionResult> CreateCheckoutSession(int courseId)
		{
			var userId = GetUserId();
			if (!User.Identity.IsAuthenticated)
			{
				string returnUrl = $"{HttpContext.Request.Path}{HttpContext.Request.QueryString}?courseId={courseId}";
				return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl });
			}
			try
			{

				var course = await _purchaseService.BeforeCheckout(userId, courseId);

				var successUrl = GetBaseUrl() + "/Payment/success?session_id={CHECKOUT_SESSION_ID}";
				var cancelUrl = GetBaseUrl() + "/Payment/cancel";
				StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

				var options = new SessionCreateOptions
				{
					PaymentMethodTypes = new List<string>
					{
						"card"
					},
					LineItems = new List<SessionLineItemOptions>
				{
					new SessionLineItemOptions
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							Currency = "usd",
							UnitAmount = Convert.ToInt32(course.Price) * 100,
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = course.Title,
								Description = course.Description
							}
						},
						Quantity = 1,

					}
				},
					Mode = "payment",
					SuccessUrl = successUrl,
					CancelUrl = cancelUrl,
					Metadata = new Dictionary<string, string>
			{
				{"userId", userId},
				{"courseName", course.Title},
				{"courseId", courseId.ToString()},
			}
				};

				var service = new SessionService();
				var session = service.Create(options);
				SessionId = session.Id;
				TempData["Session"] = SessionId;
				return Redirect(session.Url);
			}
			catch (ValidationException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
				return RedirectToAction("Index", "Courses");

			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", new { statusCode = 505 });
			}


		}

		public async Task<IActionResult> success(string session_id)
		{
			var service = new SessionService();
			var session = service.Get(session_id);
			var userId = session.Metadata.GetValueOrDefault("userId");
			var coursename = session.Metadata.GetValueOrDefault("courseName");
			if (!int.TryParse(session.Metadata.GetValueOrDefault("courseId"), out int intCourseId))
			{
				TempData["ErrorMessage"] = "Invalid courseId in metadata.";
				return RedirectToAction("Index", "Courses");
			}
			if (session.PaymentStatus == "paid")
			{
				await _purchaseService.RegisterNewPurchase(userId, intCourseId);
				TempData["SuccessMessage"] = "Purchase Succeed";
				return RedirectToAction("ChapterDetails", "Chapters", new { courseName = coursename });
			}
			TempData["ErrorMessage"] = "Purchase Failed";
			return RedirectToAction("Index", "Courses");
		}

		public IActionResult cancel()
		{
			TempData["ErrorMessage"] = "Purchase Cancelled";
			return RedirectToAction("Index", "Courses");
		}

		private string GetBaseUrl()
		{
			var request = _httpContextAccessor.HttpContext.Request;
			var baseUrl = $"{request.Scheme}://{request.Host}";

			return baseUrl;
		}
	}
}

