using Libria.Data;
using Libria.Models;
using Libria.Models.Entities;
using Libria.Services;
using Libria.ViewModels.Cart;
using Libria.ViewModels.Order;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
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

			ViewData["TotalPrice"] = orderItems.Sum(i => i.TotalItemPrice);

			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Place(OrderDetailsViewModel orderDetails)
		{
			if (ModelState.IsValid)
			{
				var orderItems = await _cartService.GetUserCartItemsAsync(HttpContext, includeAuthors: false);
				var order = new Order()
				{
					OrderDate = DateTime.UtcNow,
					TotalSpent = orderItems.Sum(i => i.TotalItemPrice),
					UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
					Email = orderDetails.Email,
					FirstName = orderDetails.FirstName,
					LastName = orderDetails.LastName,
					PhoneNumber = orderDetails.PhoneNumber,
					OrderStatus = OrderStatuses.Processing,
					Books = orderItems.Select(i => new OrdersBooks() { BookId = i.Book.BookId, Price = i.ActiveBookPrice, Quantity = i.Quantity }).ToList()
				};
				_context.Orders.Add(order);
				var res = await _context.SaveChangesAsync();

				if (res != 0)
				{
					_cartService.ClearUserCart(HttpContext);
					ViewData["OrderId"] = order.OrderId;
					return View("Success");
				}
				return RedirectToAction("Error", "Home");
			}
			else
			{
				orderDetails.CartItems = await _cartService.GetUserCartItemsAsync(HttpContext);
				return View("Index", orderDetails);
			}
		}
	}
}
