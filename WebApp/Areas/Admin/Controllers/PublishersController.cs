using Libria.Areas.Admin.Models;
using Libria.Areas.Admin.ViewModels.Authors;
using Libria.Areas.Admin.ViewModels.Publishers;
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
	public class PublishersController : Controller
	{
		private readonly LibriaDbContext _context;

		public PublishersController(LibriaDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index(string? q)
		{
			var query = _context.Publishers.AsNoTracking();
			if (q != null)
				query = query.Where(a => EF.Functions.ILike(a.Name, $"%{q}%"));

			var cards = await query
				.OrderByDescending(a => a.PublisherId)
				.Select(p => new PublisherCard { Publisher = p, ItemsCount = p.Books.Count })
				.ToListAsync();
			var viewModel = new AllPublishersViewModel(cards)
			{
				CurrentSearchString = q
			};

			return View(viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> Remove(int? publisherId = null)
		{
			if (publisherId == null)
				return BadRequest();

			var publisher = await _context.Publishers.FirstOrDefaultAsync(p => p.PublisherId == publisherId);
			if (publisher == null)
				return NotFound();

			_context.Publishers.Remove(publisher);
			await _context.SaveChangesAsync();

			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> Create(string? name)
		{
			if (string.IsNullOrEmpty(name))
				return Problem("Ім'я не може бути пустим");
			if (name.Length > 40)
				return Problem("Ім'я повинно бути не більше 40 символів");

			var publisher = new Publisher { Name = name };
			_context.Publishers.Add(publisher);
			await _context.SaveChangesAsync();

			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> Edit(string? name, int? publisherId)
		{
			if (string.IsNullOrEmpty(name))
				return Problem("Ім'я не може бути пустим");
			if (name.Length > 40)
				return Problem("Ім'я повинно бути не більше 40 символів");
			if (publisherId == null)
				return Problem("Не передано ідентифікатор видавництва");

			var publisher = await _context.Publishers.FirstOrDefaultAsync(p => p.PublisherId == (int)publisherId);

			if (publisher == null) 
				return NotFound();

			publisher.Name = name;
			await _context.SaveChangesAsync();

			return RedirectToAction("Index");
		}
	}
}
