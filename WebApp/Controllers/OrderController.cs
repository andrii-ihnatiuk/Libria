using Libria.Data;
using Libria.Models;
using Libria.Services;
using Libria.ViewModels.Cart;
using Libria.ViewModels.Order;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace Libria.Controllers
{
	public class OrderController : Controller
	{
		private readonly LibriaDbContext _context;
		private readonly IUserCartService _cartService;

		public OrderController(LibriaDbContext context, IUserCartService cartService)
		{
			_context = context;
			_cartService = cartService;
		}

		//public OrderController(IUserCartService cartService)
		//{
		//	_cartService = cartService;
		//}

		[HttpPost]
		public async Task<IActionResult> Index()
		{
			var orderItems = await _cartService.GetUserCartItemsAsync(HttpContext);
			var model = new OrderDetailsViewModel { CartItems = orderItems };

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			// Logged in user
			if (userId != null)
			{
				var user = _context.Users.Where(u => u.Id == userId).FirstOrDefault();
				if (user != null)
				{
					model.PhoneNumber = user.PhoneNumber;
					model.FirstName = user.FirstName;
					model.LastName = user.LastName;
					model.Email = user.Email;
				}
			}

			ViewData["TotalPrice"] = orderItems.Sum(i => i.FinalPrice);

			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Place(OrderDetailsViewModel order)
		{
			if (ModelState.IsValid)
			{
				return Json(order);
			}
			else
			{
				order.CartItems = await _cartService.GetUserCartItemsAsync(HttpContext);
				return View("Index", order);
			}
		}
	}
}
