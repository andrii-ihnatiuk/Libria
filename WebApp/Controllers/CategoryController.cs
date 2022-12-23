using Libria.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Libria.Controllers
{
	public class CategoryController : Controller
	{
		private readonly LibriaDbContext _context;

		public CategoryController(LibriaDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index(int? categoryId)
		{
			if (categoryId == null)
				return RedirectToAction("Error", "Home");

			var category = await _context.Categories.Where(c => c.CategoryId == categoryId).Include(c => c.Books).ThenInclude(b => b.Authors).FirstOrDefaultAsync();

			if (category == null)
				return RedirectToAction("Error", "Home");

			return View(category.Books);
		}
	}
}
