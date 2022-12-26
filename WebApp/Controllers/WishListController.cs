using Libria.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Libria.Services;

namespace Libria.Controllers
{
    public class WishListController : Controller
	{
		private readonly IWishListService _wishListService;

		public WishListController(IWishListService wishListService)
		{
			_wishListService = wishListService;
		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> Index()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (userId == null)
			{
				return RedirectToAction("Error", "Home");
			}

			var wishedBooks = await _wishListService.GetUserWishListBooksAsync(userId);

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

			return await _wishListService.AddToUserWishListAsync(userId, (int)bookId);
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Remove(int? bookId = null)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null || bookId == null)
				return Json(new { success = false });

			return await _wishListService.RemoveFromUserWishListAsync(userId, (int)bookId);
		}
	}
}
