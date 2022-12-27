using Libria.Data;
using Libria.Services;
using Libria.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Libria.Controllers
{
	public class SearchController : Controller
	{
		private readonly LibriaDbContext _context;
		private readonly IWishListService _wishListService;

		public SearchController(LibriaDbContext context, IWishListService wishListService)
		{
			_context = context;
			_wishListService = wishListService;
		}

		public async Task<IActionResult> Index(string q, int page = 1)
		{
			int pageSize = 3;
			if (string.IsNullOrEmpty(q))
			{
				return RedirectToAction("Index", "Home");
			}
			// search books by book title search string
			var booksSearch = _context.Books
				.Include(b => b.Authors)
				.Where(b => EF.Functions.ILike(b.Title, $"%{q}%")) // PostgreSql extension for case insensitive LIKE
				.Select(b => new BookCardViewModel { Book = b, Wished = false });
			// search books by author name search string
			var authorsSearch = _context.Authors
				.Include(a => a.Books)
				.ThenInclude(b => b.Authors)
				.Where(a => EF.Functions.ILike(a.Name, $"%{q}%"))
				.SelectMany(a => a.Books)
				.Select(b => new BookCardViewModel { Book = b, Wished = false });

			// distinct join two results
			var searchResult = booksSearch.Union(authorsSearch);

			// pagination
			var count = await searchResult.CountAsync();

			var pageViewModel = new PageViewModel(count, page, pageSize);

			if (page > pageViewModel.TotalPages && page != 1)
				return NotFound();

			List<BookCardViewModel> pageItems;
			if (pageViewModel.TotalPages > 0) // if we we haven't found anything there is no sense to disturb DB for data
				pageItems = await searchResult.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
			else
				pageItems = new List<BookCardViewModel>();

			IndexViewModel viewModel = new()
            {
				BookCards = pageItems,
				PageViewModel = pageViewModel
			};

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			pageItems = await _wishListService.CheckIfBooksInUserWishListAsync(userId, pageItems);
			ViewData["searchString"] = q;
			ViewData["totalItems"] = count;

			return View(viewModel);
		}
	}
}
