using Libria.Data;
using Libria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Security.Claims;

namespace Libria.Controllers
{
    public class BookController : Controller
	{
		private readonly LibriaDbContext _context;
		private readonly INotificationService _notificationService;
		private readonly ILogger<BookController> _logger;

		public BookController(LibriaDbContext context, ILogger<BookController> logger, INotificationService notificationService)
		{
			_context = context;
			_notificationService = notificationService;
			_logger = logger;
		}

		public async Task<IActionResult> Index(int? bookId = null)
		{
			if (bookId == null)
			{
				return BadRequest();
			}

			if (User.Identity != null && User.Identity.IsAuthenticated)
			{
				var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

				if (userId != null)
				{
					if (_context.WishList.Any(wl => wl.UserId == userId && wl.BookId == bookId))
					{
						ViewBag.IsInWishList = true;
					}
				}

			}

			var book = await (from b in _context.Books
							  where b.BookId == bookId
							  select b).Include(b => b.Authors).Include(b => b.Categories).FirstOrDefaultAsync();

			return book == null ? NotFound() : View(book);
		}

		[HttpPost]
		public async Task<IActionResult> NotifyMe(int? bookId, string? userEmail, NotificationType? type)
		{
			if (bookId == null || userEmail == null || type == null)
				return BadRequest();
			if (!MailAddress.TryCreate(userEmail, out _))
				return BadRequest();

			var res = await _notificationService.SubscribeForNotificationAsync(userEmail, (int)bookId, (NotificationType)type);

			return Json(new { status = res });
		}

		[HttpGet]
		public IActionResult UnsubscribeNotification(int? subscriptionId, string? userEmail, int? bookId) 
		{
			if (subscriptionId == null || userEmail == null || bookId == null)
				return BadRequest();

			NotificationRegisterStatus res = _notificationService.Unsubscribe((int)subscriptionId, userEmail, (int)bookId);
			if (res == NotificationRegisterStatus.Failed)
				return NotFound();

			return View();
		}
	}
}
