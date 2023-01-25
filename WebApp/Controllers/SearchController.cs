using Libria.Data;
using Libria.Services;
using Libria.ViewModels;
using Libria.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Libria.Controllers
{
    public class SearchController : Controller
	{
		private readonly LibriaDbContext _context;
		private readonly IWishListService _wishListService;
		private readonly ISearchService _searchService;

		public SearchController(LibriaDbContext context, IWishListService wishListService, ISearchService searchService)
		{
			_context = context;
			_wishListService = wishListService;
			_searchService = searchService;
		}

		public async Task<IActionResult> Index(string q, string sortBy = SortState.Default, int page = 1)
		{
			int pageSize = 6;
			var searchResult = await _searchService.SearchAsync(q: q, page: page, sortBy: sortBy, pageSize: pageSize);
			if (searchResult == null)
				return NotFound();

			var pageViewModel = new PageViewModel(searchResult.ResultsCount, page, pageSize);
			var pageItems = searchResult.Data;

			IndexViewModel viewModel = new()
            {
				BookCards = pageItems,
				PageViewModel = pageViewModel,
				FilterViewModel = new FilterViewModel { SearchString = q },
				ControllerName = "Search",
				ActionName = "Index",
				CurrentSortState = sortBy
			};

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			await _wishListService.CheckIfBooksInUserWishListAsync(userId, pageItems);

			return View(viewModel);
		}
	}
}
