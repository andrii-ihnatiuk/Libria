using Libria.Data;
using Libria.Models.Entities;
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

		[HttpGet]
		public async Task<IActionResult> Index(int? bookId = null)
		{
			if (bookId == null)
				return BadRequest();

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

			var book = await SelectBook((int)bookId);

			book.Reviews.Add(new Review { ReviewDate = DateTime.UtcNow, Username = "Ігнатюк Андрій", StarsQuantity = 3, Text = "Привіт, нещодавно прочитав цю книгу і от що я скажу, на цьому білому світі немає книги краще, дитина була дуже задоволена і увесь час із подивом слухала мої слова." });

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

		[HttpPost]
		public async Task<IActionResult> AddReview(int? bookId, string? reviewText, int? starsQuantity)
		{
			if (User.Identity?.IsAuthenticated == false || bookId == null || reviewText == null || starsQuantity == null || starsQuantity < 0 || starsQuantity > 5)
				return BadRequest();
			if (reviewText.Length > 1000)
				return Problem("Текст відгуку перевищує 1000 символів");

			var book = await SelectBook((int)bookId);
			if (book == null)
				return NotFound();

			book.Reviews.Add(new Review { ReviewDate = DateTime.UtcNow, Text = reviewText, StarsQuantity = (int)starsQuantity, Username = "Test" });
			await _context.SaveChangesAsync();

			return RedirectToAction("Index", new { bookId });
		}

		private async Task<Book?> SelectBook(int bookId)
		{
			return await _context.Books.Select(b => new Book
			{
				BookId = b.BookId,
				Available = b.Available,
				Description = b.Description,
				Isbn = b.Isbn,
				ImageUrl = b.ImageUrl,
				Pages = b.Pages,
				Price = b.Price,
				SalePrice = b.SalePrice,
				PublicationYear = b.PublicationYear,
				Quantity = b.Quantity,
				Title = b.Title,
				Publisher = b.Publisher,
				Language = b.Language,
				Authors = b.Authors.Select(a => new Author { AuthorId = a.AuthorId, Name = a.Name }).ToList(),
				Categories = b.Categories.Select(c => new Category { CategoryId = c.CategoryId, Name = c.Name }).ToList(),
				Reviews = b.Reviews.Select(r => new Review { ReviewDate = r.ReviewDate, StarsQuantity = r.StarsQuantity, Text = r.Text, Username = r.Username }).ToList()
			}).FirstOrDefaultAsync(b => b.BookId == bookId);
		}
	}
}
