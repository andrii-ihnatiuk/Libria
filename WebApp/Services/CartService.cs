using Libria.Models;
using Libria.ViewModels.Cart;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Libria.Extensions;
using Libria.Data;
using Libria.Models.Entities;
using System.Diagnostics;
using static System.Reflection.Metadata.BlobBuilder;

namespace Libria.Services
{
	public class CartService : ICartService
	{
		private readonly LibriaDbContext _context;
		private const string SES_CART_KEY = "CartItems";

		public CartService(LibriaDbContext context)
		{
			_context = context;
		}

		public async Task<List<CartItemViewModel>> GetUserCartItemsAsync(HttpContext _http, bool includeAuthors = true)
		{
			List<CartItemViewModel> cartItems;

			var userId = _http.User.FindFirstValue(ClaimTypes.NameIdentifier);
			// Anonymous user
			if (userId == null)
			{
				var sessionCartItems = _http.Session.Get<List<CartItem>>(SES_CART_KEY);
				if (sessionCartItems == null)
				{
					cartItems = new List<CartItemViewModel>();
				}
				else
				{
					var itemsIds = sessionCartItems.Select(s => s.BookId).ToList();
					
					var query = _context.Books.AsNoTracking().Where(b => itemsIds.Contains(b.BookId));
					if (includeAuthors)
					{
						query = query
							.Select(b => new Book
							{
								BookId = b.BookId,
								Title = b.Title,
								Price = b.Price,
								SalePrice = b.SalePrice,
								ImageUrl = b.ImageUrl,
								Authors = b.Authors.Select(a => new Author { Name = a.Name }).ToList()
							});
					}
					else
					{
						query = query
							.Select(b => new Book
							{
								BookId = b.BookId,
								Title = b.Title,
								Price = b.Price,
								SalePrice = b.SalePrice,
								ImageUrl = b.ImageUrl,
							});
					}
						
					cartItems = (await query.ToListAsync())
						.Join(sessionCartItems, b => b.BookId, s => s.BookId, (b, s) => new CartItemViewModel { Book = b, Quantity = s.Quantity })
						.ToList();
				}
			}
			// Logged in user
			else
			{
				var query = _context.CartUsersBooks.AsNoTracking().Where(c => c.UserId == userId);
				if (includeAuthors)
				{
					cartItems = await query
						.Select(c => new CartItemViewModel
						{
							Book = new Book
							{
								BookId = c.BookId,
								Title = c.Book.Title,
								Price = c.Book.Price,
								SalePrice = c.Book.SalePrice,
								ImageUrl = c.Book.ImageUrl,
								Authors = c.Book.Authors.Select(a => new Author { Name = a.Name }).ToList()
							},
							Quantity = c.Quantity
						})
						.ToListAsync();
				}
				else
				{
					cartItems = await query
						.Select(c => new CartItemViewModel
						{
							Book = new Book
							{
								BookId = c.BookId,
								Title = c.Book.Title,
								Price = c.Book.Price,
								SalePrice = c.Book.SalePrice,
								ImageUrl = c.Book.ImageUrl,
							},
							Quantity = c.Quantity
						})
						.ToListAsync();
				}
			}

			if (cartItems.Count > 0)
			{
				foreach (var cartItem in cartItems)
				{
					if (cartItem.Book.SalePrice < cartItem.Book.Price)
					{
						cartItem.TotalItemPrice = (decimal)cartItem.Book.SalePrice * cartItem.Quantity;
						cartItem.ActiveBookPrice = (decimal)cartItem.Book.SalePrice;
					}
					else
					{
						cartItem.TotalItemPrice = cartItem.Book.Price * cartItem.Quantity;
						cartItem.ActiveBookPrice = cartItem.Book.Price;
					}
				}
			}

			return cartItems;
		}


		public async Task<CartActionResult> AddToUserCartAsync(HttpContext _http, int? bookId)
		{
			if (bookId == null)
				return new CartActionResult { Success = false };

			var currentBook = await _context.Books
				.Where(b => b.BookId == bookId)
				.Select(b => new Book { Available = b.Available, Price = b.Price, SalePrice = b.SalePrice }).FirstOrDefaultAsync();

			if (currentBook == null)
				return new CartActionResult { Success = false, ErrorMessage = "Товар не знайдено." };
			if (currentBook.Available == false)
				return new CartActionResult { Success = false, ErrorMessage = "Немає в наявності." };

			var userId = _http.User.FindFirstValue(ClaimTypes.NameIdentifier);

			var actionResult = new CartActionResult() { BookId = (int)bookId, NewQuantity = 1 };
			List<CartItem> cartItems;

			// Anonymous user
			if (userId == null)
			{
				// Get cart of anonymous user from session
				var sessionCartItems = _http.Session.Get<List<CartItem>>(SES_CART_KEY);
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
				_http.Session.Set(SES_CART_KEY, sessionCartItems);
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

			return actionResult;
		}


		public async Task<CartActionResult> RemoveFromUserCartAsync(HttpContext _http, int? bookId, bool fullRemove)
		{
			if (bookId == null)
				return new CartActionResult { Success = false };

			var currentBook = await _context.Books
					.Where(b => b.BookId == bookId)
					.Select(b => new Book { Price = b.Price, SalePrice = b.SalePrice, Available = b.Available })
					.FirstOrDefaultAsync();

			if (currentBook == null)
				return new CartActionResult { Success = false, ErrorMessage = "Товар не знайдено." };

			var userId = _http.User.FindFirstValue(ClaimTypes.NameIdentifier);

			var actionResult = new CartActionResult() { BookId = (int)bookId, NewQuantity = 0 };
			List<CartItem> cartItems;

			// Anonymous user
			if (userId == null)
			{
				var sessionCartItems = _http.Session.Get<List<CartItem>>(SES_CART_KEY);
				var cartItem = sessionCartItems?.Find(c => c.BookId == bookId);

				// Anonymous cart is empty
				if (sessionCartItems == null || cartItem == null)
				{
					return new CartActionResult { Success = false, ErrorMessage = "No such product in cart." };
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
						return new CartActionResult { Success = false, ErrorMessage = "Quantity cannot be 0. Trigger full removal instead." };
					}
				}
				_http.Session.Set(SES_CART_KEY, sessionCartItems);
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
					return new CartActionResult { Success = false, ErrorMessage = "No such product in cart." };
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
						return new CartActionResult { Success = false, ErrorMessage = "Quantity cannot be 0. Trigger full removal instead." };
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

			return actionResult;
		}


		/* UTIL FUNCTIONS */


		private static decimal CalcTotalItemPrice(Book book, int quantity)
		{
			decimal totalPrice;
			if (book.SalePrice < book.Price)
				totalPrice = (decimal)book.SalePrice * quantity;
			else
				totalPrice = book.Price * quantity;
			return totalPrice;
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
				if (item.Book.SalePrice < item.Book.Price)
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


		public CartActionResult ClearUserCart(HttpContext _http)
		{
			var userId = _http.User.FindFirstValue(ClaimTypes.NameIdentifier);

			// Anonymous user
			if (userId == null)
			{
				_http.Session.Remove(SES_CART_KEY);
			}
			// Logged in user
			else
			{
				_context.Database.ExecuteSqlInterpolated
				(
					$@"DELETE FROM ""CartUsersBooks"" WHERE ""UserId"" = {userId};"
				);
			}

			return new CartActionResult { Success = true, TotalCartPrice = 0 };
		}
	}
}