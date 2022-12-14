using Libria.Models;
using Libria.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Libria.Controllers
{
	public class BookController : Controller
	{
		private readonly IBookRepository _repository;
		public BookController(IBookRepository repository)
		{
			_repository = repository;
		}

		public async Task<IActionResult> Index(int? bookId = null)
		{
			//var book = new Book
			//{
			//	BookId = 1,
			//	Title = "BoOok 1",
			//	Price = 1000,
			//	Authors = new List<Author>
			//	{ new Author { Name = "Andrew 1" } }
			//};
			if (bookId == null)
			{
				return RedirectToAction("Error", "Home");
			}
			var book = await _repository.GetBookByIdAsync((int)bookId);

			return book == null ? RedirectToAction("Error", "Home") : View(book);
		}
	}
}
