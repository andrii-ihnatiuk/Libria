using Libria.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Libria.Models.Entities;

namespace Libria.Controllers
{
    public class WishListController : Controller
	{
		private readonly LibriaDbContext _context;

		public WishListController(LibriaDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		[Authorize]
		public IActionResult Index()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (userId == null)
			{
				return RedirectToAction("Error", "Home");
			}

			var wishedBooks = _context.Books.Join(_context.WishList.Where(
				wl => wl.UserId == userId),
				b => b.BookId, wl => wl.BookId,
				(b, _) => b).Include(b => b.Authors).ToList();


			// OFF-LINE CODE
			//var wishedBooks = new List<Book>()
			//{
			//	new Book { BookId = 1,Title = "BoOok 1",ImageUrl = "img/book_cover/1.jpg",Price = 1000,Authors = new List<Author> { new Author { Name = "Author 1" } }  },
			//	new Book { BookId = 2,Title = "BoOok 2",ImageUrl = "img/book_cover/2.png",Price = 600,Authors = new List<Author> { new Author { Name = "Author 2" } } },
			//	new Book { BookId = 3,Title = "BoOok 3",ImageUrl = "img/book_cover/1.jpg",Price = 40,Authors = new List<Author> { new Author { Name = "Author 3" } } },
			//	new Book { BookId = 3,Title = "BoOok 3",ImageUrl = "img/book_cover/1.jpg",Price = 40,Authors = new List<Author> { new Author { Name = "Author 3" } } },
			//	new Book { BookId = 3,Title = "BoOok 3",ImageUrl = "img/book_cover/1.jpg",Price = 40,Authors = new List<Author> { new Author { Name = "Author 3" } } },
			//	new Book { BookId = 3,Title = "BoOok 3",ImageUrl = "img/book_cover/1.jpg",Price = 40,Authors = new List<Author> { new Author { Name = "Author 3" } } },
			//	new Book { BookId = 3,Title = "BoOok 3",ImageUrl = "img/book_cover/1.jpg",Price = 40,Authors = new List<Author> { new Author { Name = "Author 3" } } },
			//};

			return View(wishedBooks);
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Add(int? bookId = null)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null || bookId == null)
				return Json(new { success = false });

			if (_context.Users.Any(u => u.Id == userId) == false || _context.Books.Any(b => b.BookId == bookId) == false)
				return Json(new { success = false });
			if (_context.WishList.Any(wl => wl.UserId == userId && wl.BookId == bookId))
				return Json(new { success = false });

			_context.WishList.Add(new WishList { UserId = userId, BookId = (int)bookId });
			var res = await _context.SaveChangesAsync();

			return res == 0 ? Json(new { success = false }) : Json(new { success = true });
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Remove(int? bookId = null)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null || bookId == null)
				return Json(new { success = false });

			_context.WishList.Remove(new WishList { UserId = userId, BookId = (int)bookId });
			var res = await _context.SaveChangesAsync();

			return res == 0 ? Json(new { success = false }) : Json(new { success = true });
		}
	}
}
