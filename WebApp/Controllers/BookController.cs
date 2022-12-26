using Libria.Data;
using Libria.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Libria.Controllers
{
	public class BookController : Controller
	{
		private readonly LibriaDbContext _context;
		private readonly ILogger<BookController> _logger;
		public BookController(LibriaDbContext context, ILogger<BookController> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task<IActionResult> Index(int? bookId = null)
		{
			if (bookId == null)
			{
				return RedirectToAction("Error", "Home");
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


			// OFF-LINE CODE
			//var cats = new[] { new Category { Name = "Наука і техніка" }, new Category { Name = "Історія" } };
			//var book = new Book
			//{
			//	BookId = 1,
			//	Title = "BoOok 1",
			//	ImageUrl = "img/book_cover/1.jpg",
			//	SalePrice = 1200,
			//	Price = 1000,
			//	Categories = cats,
			//	Authors = new List<Author> { new Author { Name = "Author 1" } },
			//	Description = "Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet." +
			//"Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet" +
			//"Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet.Lorem ipsum dolor sit amet"
			//};

			return book == null ? NotFound() : View(book);
		}
	}
}
