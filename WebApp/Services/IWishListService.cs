using Libria.Models.Entities;
using Libria.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Libria.Services
{
	public interface IWishListService
	{
		public Task<List<Book>> GetUserWishListBooksAsync(string userId);

		public Task<List<int>?> GetUserWishListBooksIdsOnlyAsync(string? userId);

		public Task SetWishStatusForBookCardsAsync(string? userId, List<BookCardViewModel> books);

		public Task<JsonResult> AddToUserWishListAsync(string userId, int bookId);

		public Task<JsonResult> RemoveFromUserWishListAsync(string userId, int bookId);
	}
}
