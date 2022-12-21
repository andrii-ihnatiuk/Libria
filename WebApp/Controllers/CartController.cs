using Microsoft.AspNetCore.Mvc;
using Libria.ViewModels.Cart;
using Libria.Models;
using Libria.Models.Entities;
using Libria.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Libria.Extensions;

namespace Libria.Controllers
{
	public class CartController : Controller
	{
		private readonly LibriaDbContext _context;
		private readonly ILogger<CartController> _logger;
		private const string SES_CART_KEY = "CartItems";

		public CartController(LibriaDbContext context, ILogger<CartController> logger)
		{
			_context = context;
			_logger = logger;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			List<CartItemViewModel> cartItems;

			// OFF-LINE CODE
			//cartItems = new List<CartItemViewModel>()
			//{
			//	new CartItemViewModel
			//	{
			//		Book = new Book { Title = "Book in cart", Authors = new List<Author>() { new Author { Name = "Author #1" } }, Price = 1200, SalePrice = 1000, ImageUrl = "img/book_cover/1.jpg"  },
			//		Quantity = 2
			//	},
			//	new CartItemViewModel
			//	{
			//		Book = new Book { Title = "Book #2", Authors = new List<Author>() { new Author { Name = "Author #2" } }, Price = 500, SalePrice = 500, Available = false, ImageUrl = "img/book_cover/2.png"  },
			//		Quantity = 3
			//	}
			//};

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			// Anonymous user
			if (userId == null)
			{
				var sessionCartItems = HttpContext.Session.Get<List<CartItem>>(SES_CART_KEY);
				if (sessionCartItems == null)
				{
					cartItems = new List<CartItemViewModel>();
				}
				else
				{
					var itemsIds = sessionCartItems.Select(s => s.BookId).ToList();
					cartItems = (await _context.Books
						.Where(b => itemsIds.Contains(b.BookId))
						.Include(b => b.Authors)
						.ToListAsync())
						.Join(sessionCartItems, b => b.BookId, s => s.BookId, (b, s) => new CartItemViewModel { Book = b, Quantity = s.Quantity })
						.ToList();
				}
			}
			// Logged in user
			else
			{
				cartItems = await _context.CartUsersBooks
					.Where(c => c.UserId == userId)
					.Include(c => c.Book.Authors)
					.Select(c => new CartItemViewModel { Book = c.Book, Quantity = c.Quantity })
					.ToListAsync();
			}

			if (cartItems.Count > 0)
			{
				foreach (var cartItem in cartItems)
				{
					if (cartItem.Book.SalePrice != null && cartItem.Book.SalePrice < cartItem.Book.Price)
					{
						cartItem.FinalPrice = (decimal)cartItem.Book.SalePrice * cartItem.Quantity;
					}
					else
					{
						cartItem.FinalPrice = cartItem.Book.Price * cartItem.Quantity;
					}
				}
			}
			ViewData["TotalPrice"] = cartItems.Sum(i => i.FinalPrice);

			return View(cartItems);
		}

		[HttpPost]
		public async Task<IActionResult> Add(int? bookId)
		{
			if (bookId == null)
				return Json(new CartActionResult { Success = false });

			var currentBook = await _context.Books
				.Where(b => b.BookId == bookId)
				.Select(b => new Book { Available = b.Available, Price = b.Price, SalePrice = b.SalePrice }).FirstOrDefaultAsync();

			if (currentBook == null || currentBook.Available == false)
				return Json(new CartActionResult { Success = false });

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var actionResult = new CartActionResult() { BookId = (int)bookId, NewQuantity = 1 };
			List<CartItem> cartItems;

			// Anonymous user
			if (userId == null)
			{
				// Get cart of anonymous user from session
				var sessionCartItems = HttpContext.Session.Get<List<CartItem>>(SES_CART_KEY);
				if (sessionCartItems == null) // User's cart is empty
				{
					sessionCartItems = new List<CartItem>()
						{
							new CartItem { BookId = (int)bookId, Quantity = 1 }
						};
				}
				else // There are some items already
				{
					var item = sessionCartItems.Find(s => s.BookId == bookId);
					if (item == null) // No book with given id in the cart yet
					{
						sessionCartItems.Add(new CartItem
						{
							BookId = (int)bookId,
							Quantity = 1
						});
					}
					else // Increase quantity when book is present in the cart
					{
						item.Quantity += 1;
						actionResult.NewQuantity = item.Quantity;
					}
				}
				// Set updated cart to session
				HttpContext.Session.Set(SES_CART_KEY, sessionCartItems);
				cartItems = sessionCartItems; // Calculate total sum from this list
				actionResult.Success = true;
			}
			// Logged in user
			else
			{
				// Get full user's cart
				var userCartItems = await _context.CartUsersBooks.Where(c => c.UserId == userId).ToListAsync();

				var item = userCartItems.FirstOrDefault(c => c.BookId == bookId);

				if (item == null) // No book with given id in the cart yet
				{
					var entry = new CartUsersBooks { UserId = userId, BookId = (int)bookId };
					_context.CartUsersBooks.Add(entry); // add new item to table
					userCartItems.Add(entry); // add new item to local list
				}
				else
				{
					item.Quantity += 1; // change quantity of tracked item
					actionResult.NewQuantity = item.Quantity;
				}
				int res = _context.SaveChanges();
				// Calculate total sum from this list
				cartItems = userCartItems.Select(c => new CartItem { BookId = c.BookId, Quantity = c.Quantity }).ToList();
				actionResult.Success = res != 0;
			}

			actionResult.TotalItemPrice = CalcTotalItemPrice(currentBook, (int)actionResult.NewQuantity);
			actionResult.TotalCartPrice = await CalcTotalCartPriceAsync(cartItems);

			return Json(actionResult);
		}

		[HttpPost]
		public async Task<IActionResult> Remove(int? bookId, bool fullRemove = false)
		{
			if (bookId == null)
				return Json(new CartActionResult { Success = false });

			var currentBook = await _context.Books
					.Where(b => b.BookId == bookId)
					.Select(b => new Book { Price = b.Price, SalePrice = b.SalePrice, Available = b.Available })
					.FirstOrDefaultAsync();

			if (currentBook == null || !currentBook.Available)
				return Json(new CartActionResult { Success = false, ErrorMessage = "Book is not available." });

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var actionResult = new CartActionResult() { BookId = (int)bookId, NewQuantity = 0 };
			List<CartItem> cartItems;

			// Anonymous user
			if (userId == null)
			{
				var sessionCartItems = HttpContext.Session.Get<List<CartItem>>(SES_CART_KEY);
				var cartItem = sessionCartItems?.Find(c => c.BookId == bookId);

				// Anonymous cart is empty
				if (sessionCartItems == null || cartItem == null)
				{
					return Json(new CartActionResult { Success = false, ErrorMessage = "No such product in cart." });
				}
				if (fullRemove)
				{
					sessionCartItems.RemoveAll(c => c.BookId == bookId);
				}
				else
				{
					if (cartItem.Quantity > 1)
					{
						cartItem.Quantity -= 1;
						actionResult.NewQuantity = cartItem.Quantity;
					}
					else
					{
						return Json(new CartActionResult { Success = false, ErrorMessage = "Quantity cannot be 0. Trigger full removal instead." });
					}
				}
				HttpContext.Session.Set(SES_CART_KEY, sessionCartItems);
				cartItems = sessionCartItems; // Calculate total sum from this list
				actionResult.Success = true;
			}
			else
			{
				// Get full user's cart
				var userCartItems = await _context.CartUsersBooks.Where(c => c.UserId == userId).ToListAsync();
				var cartItem = userCartItems.Find(c => c.BookId == bookId);

				if (cartItem == null)
				{
					return Json(new CartActionResult { Success = false, ErrorMessage = "No such product in cart." });
				}
				if (fullRemove)
				{
					_context.CartUsersBooks.Remove(cartItem);
					userCartItems.Remove(cartItem); // remove local copy
				}
				else
				{
					if (cartItem.Quantity > 1)
					{
						cartItem.Quantity -= 1;
						actionResult.NewQuantity = cartItem.Quantity;
					}
					else
					{
						return Json(new CartActionResult { Success = false, ErrorMessage = "Quantity cannot be 0. Trigger full removal instead." });
					}
				}
				int res = _context.SaveChanges();
				// Calculate total sum from this list
				cartItems = userCartItems.Select(c => new CartItem { BookId = c.BookId, Quantity = c.Quantity }).ToList();
				actionResult.Success = res != 0;
			}

			if (actionResult.NewQuantity != 0)
			{
				actionResult.TotalItemPrice = CalcTotalItemPrice(currentBook, (int)actionResult.NewQuantity);
			}
			actionResult.TotalCartPrice = await CalcTotalCartPriceAsync(cartItems);

			return Json(actionResult);
		}


		/* UTIL FUNCTIONS */


		private decimal CalcTotalItemPrice(Book book, int quantity)
		{
			decimal newPrice;
			if (book.SalePrice != null && book.SalePrice < book.Price)
				newPrice = (decimal)book.SalePrice * quantity;
			else
				newPrice = book.Price * quantity;
			return newPrice;
		}

		private async Task<decimal> CalcTotalCartPriceAsync(List<CartItem> cartItems)
		{
			var itemsIds = cartItems.Select(c => c.BookId);
			var cartBooks = (await _context.Books.Where(b => itemsIds.Contains(b.BookId))
				.ToListAsync())
				.Join(cartItems, b => b.BookId, i => i.BookId, (b, i) => new CartItemViewModel { Book = b, Quantity = i.Quantity });

			decimal totalCartPrice = 0;
			foreach (var item in cartBooks)
			{
				if (item.Book.SalePrice != null && item.Book.SalePrice < item.Book.Price)
				{
					totalCartPrice += (decimal)item.Book.SalePrice * item.Quantity;
				}
				else
				{
					totalCartPrice += item.Book.Price * item.Quantity;
				}
			}
			return totalCartPrice;
		}
	}
}