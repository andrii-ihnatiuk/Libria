using Microsoft.AspNetCore.Mvc;
using Libria.ViewModels.Cart;
using Libria.Data;
using Libria.Models.Entities;
using Libria.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;
using Org.BouncyCastle.Crypto.Prng;

namespace Libria.Controllers
{
	public class CartController : Controller
	{
		private readonly ILogger<CartController> _logger;
		private readonly IUserCartService _cartService;

		public CartController(ILogger<CartController> logger, IUserCartService cartService)
		{
			_logger = logger;
			_cartService = cartService;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			List<CartItemViewModel> cartItems;

			if (TempData["modelError"] != null)
				ModelState.AddModelError(string.Empty, (string)TempData["modelError"]!);
			if (TempData["cartItems"] != null)
			{
				var data = JsonSerializer.Deserialize<List<CartItemViewModel>>((string)TempData["cartItems"]!);
				if (data != null)
					cartItems = data;
				else
					cartItems = await _cartService.GetUserCartItemsAsync(HttpContext);
			}
			else
				cartItems = await _cartService.GetUserCartItemsAsync(HttpContext);
				
			ViewData["TotalPrice"] = cartItems.Sum(i => i.TotalItemPrice);

			return View(cartItems);
		}

		[HttpPost]
		public async Task<IActionResult> Add(int? bookId)
		{
			var actionResult = await _cartService.AddToUserCartAsync(HttpContext, bookId);
			return Json(actionResult);
		}

		[HttpPost]
		public async Task<IActionResult> Remove(int? bookId, bool fullRemove = false)
		{
			var actionResult = await _cartService.RemoveFromUserCartAsync(HttpContext, bookId, fullRemove);
			return Json(actionResult);
		}

		[HttpPost]
		public IActionResult Clear()
		{
			return Json(_cartService.ClearUserCart(HttpContext));
		}
	}
}