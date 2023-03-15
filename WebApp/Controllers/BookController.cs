using Libria.Data;
using Libria.Models.Entities;
using Libria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.Json;

namespace Libria.Controllers
{
	public class BookController : Controller
	{
		private readonly LibriaDbContext _context;
		private readonly INotificationService _notificationService;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IHostEnvironment _hostEnvironment;

		public BookController(
			LibriaDbContext context, 
			INotificationService notificationService, 
			IHttpClientFactory httpClientFactory, 
			IHostEnvironment hostEnvironment)
		{
			_context = context;
			_notificationService = notificationService;
			_httpClientFactory = httpClientFactory;
			_hostEnvironment = hostEnvironment;
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

			// get orders which contain current book and at least one more other
			var ordIds = await _context.OrdersBooks
				.Where(ob => ob.BookId == bookId && _context.OrdersBooks.Any(i => i.OrderId == ob.OrderId && i.BookId != bookId))
				.Select(ob => ob.OrderId)
				.ToListAsync();
			// sort those books by times they show up in other orders along with the current book and get top 10 ids
			var booksIds = await _context.OrdersBooks
				.Where(ob => ordIds.Contains(ob.OrderId) && ob.BookId != bookId)
				.GroupBy(ob => ob.BookId)
				.Select(x => new { BookId = x.Key, Count = x.Count() })
				.OrderByDescending(a => a.Count)
				.Select(x => x.BookId)
				.Take(10)
				.ToListAsync();
			// finally select those top 10 co-selling books
			ViewData["OftenBoughtTogether"] = await _context.Books
				.AsNoTracking()
				.Where(b => booksIds.Contains(b.BookId))
				.Select(b => new Book
				{
					BookId = b.BookId,
					Available = b.Available,
					ImageUrl = b.ImageUrl,
					Price = b.Price,
					SalePrice = b.SalePrice,
					Quantity = b.Quantity,
					Title = b.Title,
					Authors = b.Authors.Select(a => new Author { Name = a.Name }).OrderBy(a => a.Name).ToList(),
				})
				.OrderBy(b => booksIds.IndexOf(b.BookId))
				.ToListAsync();

			return book == null ? NotFound() : View(book);
		}

		[HttpPost]
		public async Task<IActionResult> NotifyMe(int? bookId, string? userEmail, NotificationType? type)
		{
			if (bookId == null || userEmail == null || type == null)
				return BadRequest();
			if (!MailAddress.TryCreate(userEmail, out _))
				return BadRequest();

			var res = await _notificationService.SubscribeForNotificationAsync(userEmail.Trim(), (int)bookId, (NotificationType)type);

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

			_context.Books.Attach(book);

			var userName = await _context.Users
				.Where(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier))
				.Select(u => string.Concat(u.FirstName, " ", u.LastName))
				.FirstOrDefaultAsync();

			book.Reviews.Add(new Review { ReviewDate = DateTime.UtcNow, Text = reviewText.Trim(), StarsQuantity = (int)starsQuantity, Username = userName ?? "" });
			await _context.SaveChangesAsync();

			return RedirectToAction("Index", new { bookId });
		}

		[HttpPost]
		public async Task<IActionResult> SimilarBooks(int? bookId)
		{
			if (bookId == null)
				return Problem("Не передано ідентифікатор");
			if (await _context.Books.AnyAsync(b => b.BookId == bookId) == false)
				return Problem("Передано невірний ідентифіктор");

			var httpClient = _httpClientFactory.CreateClient();
			var contentType = new MediaTypeWithQualityHeaderValue("application/json");
			httpClient.DefaultRequestHeaders.Accept.Add(contentType);

			string baseUrl = _hostEnvironment.IsProduction() ? "flask" : "localhost";
			string url = $"http://{baseUrl}:7007/content_based/{bookId}?amount=10";

			HttpResponseMessage httpResponse;
			try
			{
				httpResponse = httpClient.GetAsync(url).Result;
			}
			catch (Exception)
			{
				return Problem("Неможливо виконати запит");
			}
			if (httpResponse.IsSuccessStatusCode)
			{
				if (httpResponse.Content is not null && httpResponse.Content.Headers?.ContentType?.MediaType == "application/json")
				{
					var stream = await httpResponse.Content.ReadAsStreamAsync();

					try
					{
						var result = await JsonSerializer.DeserializeAsync<FlaskResponse>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
						if (result != null)
						{
							if (result.Success)
							{
								var books = await _context.Books.Where(b => result.Data.Contains(b.BookId)).Include(b => b.Authors).ToListAsync();
								// preserve original order of books (api returns the most similar books first but sql select is unpredictable)
								books = books.OrderBy(b => result.Data.IndexOf(b.BookId)).ToList();

								return PartialView("_BooksSliderPartial", books);
							}
							return NotFound();
						}
						return Problem("Помилка сервера");
					}
					catch (JsonException)
					{
						return Problem("Помилка сервера");
					}
				}
				else
				{
					return Problem("Отримано неприйнятний відгук");
				}
			}
			else
			{
				return Problem("Неможливо виконати запит");
			}
		}

		private async Task<Book?> SelectBook(int bookId)
		{
			return await _context.Books.AsNoTracking()
				.Select(b => new Book
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
					Authors = b.Authors.Select(a => new Author { AuthorId = a.AuthorId, Name = a.Name }).OrderBy(a => a.Name).ToList(),
					Categories = b.Categories.Select(c => new Category { CategoryId = c.CategoryId, Name = c.Name }).OrderBy(c => c.CategoryId).ToList(),
					Reviews = b.Reviews.Select(r => new Review { Id = r.Id, ReviewDate = r.ReviewDate, StarsQuantity = r.StarsQuantity, Text = r.Text, Username = r.Username }).OrderByDescending(r => r.ReviewDate).ToList()
				}).FirstOrDefaultAsync(b => b.BookId == bookId);
		}
	}
}
