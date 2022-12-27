using Libria.Data;
using Libria.Services;
using Libria.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Libria.Controllers
{
	public class CategoryController : Controller
	{
		private readonly LibriaDbContext _context;
		private readonly IWishListService _wishListService;

		public CategoryController(LibriaDbContext context, IWishListService wishListService)
		{
			_context = context;
			_wishListService = wishListService;
		}

		public async Task<IActionResult> Index(int? categoryId)
		{
			if (categoryId == null)
				return RedirectToAction("Error", "Home");

			var category = await _context.Categories.Where(c => c.CategoryId == categoryId).Include(c => c.Books).ThenInclude(b => b.Authors).FirstOrDefaultAsync();

			if (category == null)
				return NotFound();

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var bookCards = category.Books.Select(b => new BookCardViewModel { Book = b, Wished = false }).ToList();

			bookCards = await _wishListService.CheckIfBooksInUserWishListAsync(userId, bookCards);

			ViewData["categoryName"] = category.Name;
			ViewData["categoryDescription"] = category.Description;

			return View(bookCards);
		}
	}
}
