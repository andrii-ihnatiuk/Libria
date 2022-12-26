using Libria.Data;
using Libria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Libria.Controllers
{
	public class AuthorController : Controller
	{
		private readonly LibriaDbContext _context;
		private readonly IWishListService _wishListService;

		public AuthorController(LibriaDbContext context, IWishListService wishListService)
		{
			_context = context;
			_wishListService = wishListService;
		}

		public async Task<IActionResult> Index(int? authorId)
		{
			if (authorId == null)
				return RedirectToAction("Error", "Home");

			var author = await _context.Authors
				.Where(a => a.AuthorId == authorId)
				.Include(a => a.Books)
				.ThenInclude(b => b.Authors)
				.FirstOrDefaultAsync();
				
			if (author == null)
				return NotFound();

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			ViewData["wishIds"] = await _wishListService.GetUserWishListBooksIdsOnlyAsync(userId);

			return View(author);
		}
	}
}
