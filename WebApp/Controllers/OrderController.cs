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
		private readonly ICartService _cartService;

		public OrderController(LibriaDbContext context, ICartService cartService)
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
			if (orderItems.Any(o => o.Book.Available == false)) // If cart contain items that are not available
			{
				TempData["cartItems"] = JsonSerializer.Serialize(orderItems); // save data to reduce db queries
				TempData["modelError"] = "Не всі товари у кошику в наявності. Видаліть їх та спробуйте ще раз."; // add model error to display in cart view
				return RedirectToAction("Index", "Cart"); // redirect to cart view
			}
			if (orderItems.Count == 0)
			{
				TempData["cartItems"] = JsonSerializer.Serialize(orderItems); // save data to reduce db queries
				TempData["modelError"] = "Кошик пустий, мерщій за покупками!"; // add model error to display in cart view
				return RedirectToAction("Index", "Cart");
			}

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
					model.City = user.City ?? string.Empty;
					model.Address = user.Address ?? string.Empty;
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
					OrderStatus = OrderStatus.Pending,
					City = orderDetails.City,
					Address = orderDetails.Address,
					Comment = orderDetails.Comment,
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
