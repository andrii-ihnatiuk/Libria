using Libria.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Libria.Components
{
	public class CategoriesMenuViewComponent : ViewComponent
	{
		//private readonly LibriaDbContext _context;

		//public CategoriesMenuViewComponents(LibriaDbContext context)
		//{
		//	_context = context;
		//}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			//var categories = await _context.Categories.Select(c => c.Name).ToListAsync();

			var categories = new List<string>() { "Category #1", "Category #2", "Test category", "Super category", "One more thing", "Category #1", "Category #2", "Test category", "Super category", "One more thing", "Category #1", "Category #2", "Test category", "Super category", "One more thing", "Category #1", "Category #2", "Test category", "Super category", "One more thing" };

			return View(categories);
		}



	}
}
