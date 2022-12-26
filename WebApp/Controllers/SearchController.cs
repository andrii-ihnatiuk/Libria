using Libria.Data;
using Libria.Models.Entities;
using Libria.Services;
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

		public async Task<IActionResult> Index(string q)
		{
			if (string.IsNullOrEmpty(q))
			{
				return RedirectToAction("Index", "Home");
			}
			// search books by book title search string
			var booksSearch = await _context.Books
				.Include(b => b.Authors)
				.Where(b => EF.Functions.ILike(b.Title, $"%{q}%")) // PostgreSql extension for case insesitive LIKE
				.ToListAsync();
			// search books by author name search string
			var authorsSearch = await _context.Authors
				.Include(a => a.Books)
				.ThenInclude(b => b.Authors)
				.Where(a => EF.Functions.ILike(a.Name, $"%{q}%"))
				.SelectMany(a => a.Books)
				.ToListAsync();
			// distinct join two results
			var searchResult = booksSearch.UnionBy(authorsSearch, b => b.BookId).ToList();

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			ViewData["wishIds"] = await _wishListService.GetUserWishListBooksIdsOnlyAsync(userId);
			ViewData["searchString"] = q;

			return View(searchResult);
		}
	}
}
