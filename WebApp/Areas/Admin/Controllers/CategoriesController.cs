using Libria.Areas.Admin.Models;
using Libria.Areas.Admin.ViewModels.Categories;
using Libria.Data;
using Libria.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Libria.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = "admin")]
	public class CategoriesController : Controller
	{
		private readonly LibriaDbContext _context;

		public CategoriesController(LibriaDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index(string? q)
		{
			var query = _context.Categories.AsNoTracking();
			if (q != null)
				query = query.Where(c => EF.Functions.ILike(c.Name, $"%{q}%"));

			var categoryCards = await query
				.OrderByDescending(c => c.CategoryId)
				.Select(c => new CategoryCard { Category = c, ItemsCount = c.Books.Count })
				.ToListAsync();
			var viewModel = new AllCategoriesViewModel(categoryCards)
			{
				CurrentSearchString = q
			};

			return View(viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> Remove(int? categoryId = null)
		{
			if (categoryId == null)
				return BadRequest();

			var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
			if (category == null)
				return NotFound();

			_context.Categories.Remove(category);
			await _context.SaveChangesAsync();

			return RedirectToAction("Index");
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int? id = null)
		{
			if (id == null)
				return BadRequest();

			var category = await _context.Categories
				.AsNoTracking()
				.FirstOrDefaultAsync(c => c.CategoryId == id);
			if (category == null)
				return NotFound();

			var viewModel = new EditCategoryViewModel 
			{ 
				Id = category.CategoryId,  
				Name = category.Name, 
				Description = category.Description 
			};

			return View(viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(EditCategoryViewModel model)
		{
			if (ModelState.IsValid)
			{
				if (model.Id == null)
					return BadRequest();
				var category = _context.Categories.FirstOrDefault(c => c.CategoryId == model.Id);
				if (category == null)
					return NotFound();

				category.Name = model.Name.Trim();
				category.Description = model.Description?.Trim();

				await _context.SaveChangesAsync();
				return RedirectToAction("Index");
			}
			return View(model);
		}

		[HttpGet]
		public IActionResult Create()
		{
			return View("Edit", new EditCategoryViewModel { ActionName = "Create", PageTitle = "Створення категорії" });
		}

		[HttpPost]
		public async Task<IActionResult> Create(EditCategoryViewModel model)
		{
			if (ModelState.IsValid)
			{
				var category = new Category { Name = model.Name.Trim(), Description = model.Description?.Trim() };

				_context.Categories.Add(category);

				await _context.SaveChangesAsync();
				return RedirectToAction("Index");
			}
			return View("Edit", model);
		}
	}
}
