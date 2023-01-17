using Libria.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Libria.Services;
using Libria.Models.Entities;

namespace Libria.Components
{
	public class NewBooksViewComponent : ViewComponent
	{
		private readonly LibriaDbContext _context;
		private readonly IWishListService _wishListService;

		public NewBooksViewComponent(LibriaDbContext context, IWishListService wishListService)
		{
			_context = context;
			_wishListService = wishListService;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var books = await _context.Books.Select(b => new Book
			{
				BookId = b.BookId,
				Title = b.Title,
				Available = b.Available,
				Price = b.Price,
				SalePrice = b.SalePrice,
				ImageUrl = b.ImageUrl,
				Authors = b.Authors.Select(a => new Author { Name = a.Name }).ToList()
			}).OrderByDescending(b => b.BookId).Take(10).ToListAsync();

			var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

			List<int>? wishIds = null;
			if (userId != null)
				wishIds = await _wishListService.GetUserWishListBooksIdsOnlyAsync(userId);

			ViewData["wishIds"] = wishIds;

			return View(books);
		}
	}
}
