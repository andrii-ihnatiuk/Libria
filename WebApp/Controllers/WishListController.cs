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
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (userId == null)
			{
				return RedirectToAction("Error", "Home");
			}

			var wishedBooks = await _wishListService.GetUserWishListBooksAsync(userId);

			return View(wishedBooks);
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Add(int? bookId = null)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null || bookId == null)
				return Json(new { success = false });

			return await _wishListService.AddToUserWishListAsync(userId, (int)bookId);
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Remove(int? bookId = null)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null || bookId == null)
				return Json(new { success = false });

			return await _wishListService.RemoveFromUserWishListAsync(userId, (int)bookId);
		}
	}
}
