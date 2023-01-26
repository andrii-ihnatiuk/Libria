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
			var categories = await _context.Categories.AsNoTracking()
				.Select(c => new Category { CategoryId = c.CategoryId, Name = c.Name })
				.ToListAsync();

			return View(categories);
		}
	}
}
