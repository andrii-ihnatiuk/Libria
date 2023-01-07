using Libria.Data;
using Libria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

		public async Task<IActionResult> NotifyPriceDrop(int? bookId, string? userEmail)
		{
			if (bookId == null || userEmail == null)
				return BadRequest();

			var res = await _notificationService.SubscribeForNotificationAsync(userEmail, (int)bookId, NotificationType.PriceDrop);

			return Json(new { status = res });
		}
	}
}
