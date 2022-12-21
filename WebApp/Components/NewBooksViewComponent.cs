using Libria.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Libria.Models;
using System.Security.Claims;
using Libria.ViewModels;

namespace Libria.Components
{
	public class NewBooksViewComponent : ViewComponent
	{
		private readonly LibriaDbContext _context;

		public NewBooksViewComponent(LibriaDbContext context)
		{
			_context = context;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var books = await (from book in _context.Books
							   orderby book.BookId descending
							   select book).Take(10).Include(b => b.Authors).ToListAsync();
			var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

			List<int>? wishIds = null;
			if (userId != null)
				wishIds = await _context.WishList.Where(wl => wl.UserId == userId).Select(wl => wl.BookId).ToListAsync();

			ViewData["wishIds"] = wishIds;


			//var books = new List<Book>()
			//{
			//	new Book { BookId = 1, Title = "BoOok 1", SalePrice=800, Price = 1000, ImageUrl="img/book_cover/1.jpg", Authors= new List<Author> { new Author { Name = "Author 1" } } },
			//	new Book { BookId = 2, Title = "BoOok #2", SalePrice=500, Available=false, ImageUrl="img/book_cover/2.png", Price = 500, Authors= new List<Author> { new Author { Name = "Author 1" } } }
			//};

			return View(books);
		}
	}
}
