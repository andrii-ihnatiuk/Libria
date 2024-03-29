﻿using Libria.Data;
using Libria.Models.Entities;
using Libria.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Libria.Services
{
	public class WishListService : IWishListService
	{
		private readonly LibriaDbContext _context;

		public WishListService(LibriaDbContext context)
		{
			_context = context;
		}

		public async Task<List<Book>> GetUserWishListBooksAsync(string userId)
		{
			return await _context.Books.AsNoTracking()
				.Join(_context.WishList.Where(wl => wl.UserId == userId),
				b => b.BookId, wl => wl.BookId,
				(b, _) => b).Select(b => new Book 
				{ 
					BookId = b.BookId,
					Title = b.Title,
					Available = b.Available,
					ImageUrl = b.ImageUrl,
					Price = b.Price,
					SalePrice = b.SalePrice,
					Authors = b.Authors.Select(a => new Author { Name = a.Name }).ToList(),
				}).ToListAsync();
		}

		public async Task<List<int>?> GetUserWishListBooksIdsOnlyAsync(string? userId)
		{
			if (userId == null) return null;
			return await _context.WishList.Where(wl => wl.UserId == userId).Select(wl => wl.BookId).ToListAsync();
		}

		public async Task<JsonResult> AddToUserWishListAsync(string userId, int bookId)
		{
			if (_context.Users.Any(u => u.Id == userId) == false || _context.Books.Any(b => b.BookId == bookId) == false)
				return new JsonResult(new { success = false });
			if (_context.WishList.Any(wl => wl.UserId == userId && wl.BookId == bookId))
				return new JsonResult(new { success = false });

			_context.WishList.Add(new WishList { UserId = userId, BookId = bookId });
			var res = await _context.SaveChangesAsync();

			return res == 0 ? new JsonResult(new { success = false }) : new JsonResult(new { success = true });
		}

		public async Task<JsonResult> RemoveFromUserWishListAsync(string userId, int bookId)
		{
			var record = await _context.WishList.FirstOrDefaultAsync(wl => wl.UserId == userId && wl.BookId == bookId);
			if (record == null)
				return new JsonResult(new { success = false });
			_context.WishList.Remove(record);
			var res = await _context.SaveChangesAsync();

			return res == 0 ? new JsonResult(new { success = false }) : new JsonResult(new { success = true });
		}

		public async Task SetWishStatusForBookCardsAsync(string? userId, List<BookCardViewModel> bookCards)
		{
			var wishIds = await GetUserWishListBooksIdsOnlyAsync(userId);
			foreach(var bookCard in bookCards)
			{
				if (wishIds == null)
					bookCard.Wished = false;
				else if (wishIds.Contains(bookCard.Book.BookId))
					bookCard.Wished = true;
				else
					bookCard.Wished = false;
			}
		}
	}
}
