﻿using Libria.Data;
using Libria.Services;
using Libria.ViewModels;
using Libria.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Libria.Controllers
{
    public class AuthorController : Controller
	{
		private readonly LibriaDbContext _context;
		private readonly ISearchService _searchService;
		private readonly IWishListService _wishListService;

		public AuthorController(LibriaDbContext context, ISearchService searchService, IWishListService wishListService)
		{
			_context = context;
			_searchService = searchService;
			_wishListService = wishListService;
		}

		public async Task<IActionResult> Index(int? authorId, string sortBy = SortState.Default, int page = 1)
		{
			if (authorId == null)
				return RedirectToAction("Error", "Home");

			var author = _context.Authors.Where(a => a.AuthorId == authorId).FirstOrDefault();
			if (author == null)
				return NotFound();

			int pageSize = 6;
			var searchResult = await _searchService.SearchAsync(authorId: authorId, sortBy: sortBy, page: page, pageSize: pageSize);
			if (searchResult == null)
				return NotFound();

			var pageViewModel = new PageViewModel(searchResult.ResultsCount, page, pageSize);
			var pageItems = searchResult.Data;

			IndexViewModel viewModel = new()
			{
				BookCards = pageItems,
				PageViewModel = pageViewModel,
				FilterViewModel = new FilterViewModel { AuthorId = authorId },
				ControllerName = "Author",
				ActionName = "Index",
				CurrentSortState = sortBy
			};

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			await _wishListService.SetWishStatusForBookCardsAsync(userId, searchResult.Data);

			ViewData["AuthorName"] = author.Name;
			ViewData["AuthorDescription"] = author.Description;

			return View(viewModel);
		}
	}
}