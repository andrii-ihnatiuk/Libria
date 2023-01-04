using Libria.Areas.Admin.ViewModels;
using Libria.Data;
using Libria.Models.Entities;
using Libria.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Libria.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = "admin")]
	public class ProductsController : Controller
    {
        private readonly LibriaDbContext _context;

        public ProductsController(LibriaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int category = -1, string? q = null, int page = 1)
        {
            int pageSize = 3;

            var query = _context.Books.AsNoTracking();

			if (category != -1)
                query = query.Where(b => b.Categories.Any(c => c.CategoryId == category));
            if (q != null)
                query = query.Where(b => EF.Functions.ILike(b.Title, $"%{q}%"));

            query = query
                .Select(b => new Book
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Available = b.Available,
                    Price = b.Price,
                    SalePrice = b.SalePrice,
                    ImageUrl = b.ImageUrl,
                    Quantity = b.Quantity,
                    Categories = b.Categories.Select(c => new Category { Name = c.Name }).ToList(),
                    Authors = b.Authors.Select(a => new Author { Name = a.Name }).ToList()
                });

            int itemsCount = await query.CountAsync();

			var pageViewModel = new PageViewModel(itemsCount, page, pageSize);

			// In case there is 0 results just output
			if (page > pageViewModel.TotalPages && pageViewModel.TotalPages != 0 || page < 1)
				return NotFound();

            List<Book> products;
            if (pageViewModel.TotalPages > 0)
            {
                products = await query
                    .OrderByDescending(p => p.BookId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            else
                products = new();

            List<SelectListItem> categorySelectList = new() { new SelectListItem { Text = "Усі", Value = "-1" } };

			categorySelectList.AddRange(await _context.Categories
                .AsNoTracking()
                .Select(c => new SelectListItem { Text = c.Name, Value = c.CategoryId.ToString() })
                .OrderBy(c => c.Text)
                .ToListAsync());

			var viewModel = new DashboardProductsViewModel(products, pageViewModel)
            {
                CategorySelectItems = categorySelectList,
                CurrentCategoryId = category,
                CurrentSearchString = q,
                TotalItemsFound = itemsCount
			};

            return View(viewModel);
        }

        public async Task<IActionResult> Remove(int? productId = null)
        {
            if (productId == null)
                return BadRequest();

            var book = await _context.Books
                .Select(b => new Book { BookId = b.BookId })
                .FirstOrDefaultAsync(b => b.BookId == productId);

            if (book == null)
                return NotFound();

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int? id = null)
        {
            if (id == null)
                return BadRequest();

            var book = await _context.Books
                .AsNoTracking()
                .Select(b => new Book
                {
                    BookId = b.BookId,
					Title = b.Title,
					Description = b.Description,
					Available = b.Available,
					ImageUrl = b.ImageUrl,
					Isbn = b.Isbn,
					Language = b.Language,
					Pages = b.Pages,
					Price = b.Price,
					SalePrice = b.SalePrice,
					Publisher = b.Publisher,
					Quantity = b.Quantity,
					PublicationYear = b.PublicationYear,

                    Authors = b.Authors.Select(a => new Author { AuthorId = a.AuthorId }).ToList(),
                    Categories = b.Categories.Select(c => new Category { CategoryId = c.CategoryId }).ToList()
				})
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
                return NotFound();

            // Select book's active relations
            var authorSelectItems = await _context.Authors
                .Select(a => new SelectListItem { Text = a.Name, Value = a.AuthorId.ToString() })
                .ToListAsync();
            var categorySelectItems = await _context.Categories
                .Select(c => new SelectListItem { Text = c.Name, Value = c.CategoryId.ToString() })
                .ToListAsync();

            // Set items as selected if book is already related to given entity
			authorSelectItems.ForEach(i =>
			{
				if (book.Authors.Any(a => a.AuthorId == int.Parse(i.Value)))
					i.Selected = true;
			});
            categorySelectItems.ForEach(i =>
            {
                if (book.Categories.Any(c => c.CategoryId == int.Parse(i.Value)))
                    i.Selected = true;
            });

			var viewModel = new DashboardProductViewModel()
            {
                Title = book.Title,
                Description = book.Description,
                Available = book.Available,
                ImageUrl = book.ImageUrl,
                Isbn = book.Isbn,
                Language = book.Language,
                Pages = book.Pages,
                Price = book.Price,
                SalePrice = book.SalePrice,
                Publisher = book.Publisher,
                Quantity = book.Quantity,
                PublicationYear = book.PublicationYear,

                AuthorSelectItems = authorSelectItems,
                CategorySelectItems = categorySelectItems
			};

            return View(viewModel);
        }
    }
}
