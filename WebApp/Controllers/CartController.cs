using Microsoft.AspNetCore.Mvc;
using Libria.ViewModels.Cart;
using Libria.Data;
using Libria.Models.Entities;
using Libria.Services;

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