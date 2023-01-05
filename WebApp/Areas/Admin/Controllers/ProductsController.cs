using Libria.Areas.Admin.Models;
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

		[HttpPost]
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

		[HttpGet]
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
				BookId = book.BookId,
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

		[HttpPost]
		public async Task<IActionResult> Edit(DashboardProductViewModel model)
		{
			if (model.BookId == null)
				return BadRequest();

			if (ModelState.IsValid)
			{
				var book = await _context.Books
					.Include(b => b.Authors)
					.Include(b => b.Categories)
					.FirstOrDefaultAsync(b => b.BookId == model.BookId);

				if (book == null)
					return NotFound();


				// Update product image
				var fileSaveResult = await SaveProductImage(model.FileUpload, book.ImageUrl);

				switch (fileSaveResult.Status)
				{
					case FileSaveStatus.Saved:
						book.ImageUrl = fileSaveResult.FileUrl;
						break;
					case FileSaveStatus.EmptyFile:
					case FileSaveStatus.LargeFile:
						ModelState.AddModelError(nameof(model.FileUpload), fileSaveResult.ErrorMessage);
						ViewBag.FileUploadError = true;
						return View(model);
					case FileSaveStatus.Error:
						return Problem(fileSaveResult.ErrorMessage);
					default:
						return Problem("Сталася невідома помилка.");
				}

				// Update produc authors and categories
				List<Author> newAuthors;
				List<Category> newCategories;

				if (model.SelectedAuthors.Count > 0)
					newAuthors = await _context.Authors
						.Where(a => model.SelectedAuthors.Contains(a.AuthorId))
						.ToListAsync();
				else
					newAuthors = new();

				if (model.SelectedCategories.Count > 0)
					newCategories = await _context.Categories
						.Where(c => model.SelectedCategories.Contains(c.CategoryId))
						.ToListAsync();
				else
					newCategories = new();

				book.Authors = newAuthors;
				book.Categories = newCategories;


				// Update properties
				book.Title = model.Title;
				book.Description = model.Description;
				book.Price = model.Price;
				book.SalePrice = model.SalePrice == null || model.SalePrice > model.Price ? model.Price : model.SalePrice;
				book.Pages = model.Pages;
				book.Quantity = model.Quantity;
				book.Available = model.Available;
				book.Isbn = model.Isbn;
				book.Publisher = model.Publisher;
				book.PublicationYear = model.PublicationYear;
				book.Language = model.Language;

				await _context.SaveChangesAsync();

				return RedirectToAction("Edit");

			}
			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> Create()
		{
			var authorSelectItems = await _context.Authors
				.Select(a => new SelectListItem { Text = a.Name, Value = a.AuthorId.ToString() })
				.ToListAsync();
			var categorySelectItems = await _context.Categories
				.Select(c => new SelectListItem { Text = c.Name, Value = c.CategoryId.ToString() })
				.ToListAsync();

			var viewModel = new DashboardProductViewModel
			{

				PageTitle = "Новий товар",
				ControllerName = "Products",
				ActionName = "Create",
				AuthorSelectItems = authorSelectItems,
				CategorySelectItems = categorySelectItems
			};

			return View("Edit", viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> Create(DashboardProductViewModel model)
		{
			if (ModelState.IsValid)
			{
				Book book = new()
				{
					Title = model.Title,
					Quantity = model.Quantity,
					Description = model.Description,
					Price = model.Price,
					SalePrice = model.SalePrice == null || model.SalePrice > model.Price ? model.Price : model.SalePrice,
					Pages = model.Pages,
					Available = model.Available,
					Isbn = model.Isbn,
					Publisher = model.Publisher,
					PublicationYear = model.PublicationYear,
					Language = model.Language,
					Authors = model.SelectedAuthors.Select(id => new Author { AuthorId = id }).ToList(),
					Categories = model.SelectedCategories.Select(id => new Category { CategoryId = id}).ToList()
				};

				var fileSaveResult = await SaveProductImage(model.FileUpload, book.ImageUrl);

				switch (fileSaveResult.Status)
				{
					case FileSaveStatus.Saved:
						book.ImageUrl = fileSaveResult.FileUrl;
						break;
					case FileSaveStatus.EmptyFile:
					case FileSaveStatus.LargeFile:
						ModelState.AddModelError(nameof(model.FileUpload), fileSaveResult.ErrorMessage);
						ViewBag.FileUploadError = true;
						return View("Edit", model);
					case FileSaveStatus.Error:
						return Problem(fileSaveResult.ErrorMessage);
					default:
						return Problem("Сталася невідома помилка.");
				}

				_context.Books.Attach(book);
				await _context.SaveChangesAsync();

				return RedirectToAction("Index");
			}

			return View("Edit", model);
		}

		public async Task<FileSaveResult> SaveProductImage(IFormFile? formFile, string? previousImg)
		{
			var result = new FileSaveResult();

			if (formFile != null)
			{
				if (formFile.Length > 0)
				{
					if (formFile.Length < 1000_000)
					{
						try
						{
							string fileName = Path.GetFileName(formFile.FileName);
							string uploadFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "book_cover", fileName);

							using (var stream = new FileStream(uploadFilePath, FileMode.Create))
							{
								await formFile.CopyToAsync(stream);
							}
							result.Status = FileSaveStatus.Saved;
							result.FileUrl = $"/img/book_cover/{fileName}";
						}
						catch
						{
							result.Status = FileSaveStatus.Error;
							result.ErrorMessage = "Під час збереження файла сталася помилка.";
						}
					}
					else
					{
						result.Status = FileSaveStatus.LargeFile;
						result.ErrorMessage = "Файл завеликий.";
					}
				}
				else
				{
					result.Status = FileSaveStatus.EmptyFile;
					result.ErrorMessage = "Файл пустий.";
				}
			}
			else
			{
				result.Status = FileSaveStatus.Saved;
				// if product already has an image and no new was uploaded just keep an old one
				if (previousImg != null)
					result.FileUrl = previousImg;
				// if product doesn't have an image and no new was uploaded fill with default placeholder
				else
					result.FileUrl = "/img/book_cover/no_cover.jpg";
			}

			return result;
		}

	}
}
