using Libria.Data;
using Libria.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Libria.Components
{
	public class CategoriesMenuViewComponent : ViewComponent
	{
		private readonly LibriaDbContext _context;

		public CategoriesMenuViewComponent(LibriaDbContext context)
		{
			_context = context;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var categories = await _context.Categories.Select(c => new Category { CategoryId = c.CategoryId, Name = c.Name }).ToListAsync();

			//var categories = new List<Category>()
			//{
			//	new Category { CategoryId = 1,  Name = "First category" },
			//	new Category { CategoryId = 2,  Name = "Second category" },
			//	new Category { CategoryId = 3,  Name = "Category with number 3" },
			//	new Category { CategoryId = 4,  Name = "Fourth test category" },
			//};

			return View(categories);
		}
	}
}
