using Libria.Data;
using Libria.Models.Entities;
using Libria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Libria.Components
{
	public class PopularBooksViewComponent : ViewComponent
	{
		private readonly LibriaDbContext _context;
		private readonly IWishListService _wishListService;

		public PopularBooksViewComponent(LibriaDbContext context, IWishListService wishListService)
		{
			_context = context;
			_wishListService = wishListService;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var popularIds = await _context.OrdersBooks
				.GroupBy(ob => ob.BookId)
				.Select(x => new { BookId = x.Key, Count = x.Count() })
				.OrderByDescending(x => x.Count)
				.Select(x => x.BookId)
				.Take(2)
				.ToListAsync();

			var books = await _context.Books
				.Where(b => popularIds.Contains(b.BookId))
				.Select(b => new Book
				{
					BookId = b.BookId,
					Title = b.Title,
					Available = b.Available,
					Price = b.Price,
					SalePrice = b.SalePrice,
					ImageUrl = b.ImageUrl,
					Authors = b.Authors.Select(a => new Author { Name = a.Name }).ToList()
				}).ToListAsync();

			var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

			List<int>? wishIds = null;
			if (userId != null)
				wishIds = await _wishListService.GetUserWishListBooksIdsOnlyAsync(userId);

			ViewData["wishIds"] = wishIds;

			return View(books);
		}
	}
}
