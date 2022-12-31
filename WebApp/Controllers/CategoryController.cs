using Libria.Data;
using Libria.Services;
using Libria.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Libria.Controllers
{
	public class CategoryController : Controller
	{
		private readonly LibriaDbContext _context;
		private readonly ISearchService _searchService;
		private readonly IWishListService _wishListService;

		public CategoryController(LibriaDbContext context, IWishListService wishListService, ISearchService searchService)
		{
			_context = context;
			_wishListService = wishListService;
			_searchService = searchService;
		}

		public async Task<IActionResult> Index(int? categoryId, string sortBy = SortState.Default, int page = 1)
		{
			if (categoryId == null)
				return RedirectToAction("Error", "Home");

			var category = await _context.Categories.Where(c => c.CategoryId == categoryId).FirstOrDefaultAsync();
			if (category == null)
				return NotFound();

			var searchResult = await _searchService.SearchAsync(categoryId: categoryId, sortBy: sortBy,  page: page);
			if (searchResult == null)
				return NotFound();

			ViewData["categoryName"] = category.Name;
			ViewData["categoryDescription"] = category.Description;

			var pageViewModel = new PageViewModel(searchResult.ResultsCount, page);

			IndexViewModel viewModel = new()
			{
				BookCards = searchResult.Data,
				PageViewModel = pageViewModel,
				FilterViewModel = new FilterViewModel { CategoryId = categoryId },
				ControllerName = "Category",
				ActionName = "Index",
				CurrentSortState = sortBy
			};

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			await _wishListService.CheckIfBooksInUserWishListAsync(userId, searchResult.Data);

			return View(viewModel);
		}
	}
}
