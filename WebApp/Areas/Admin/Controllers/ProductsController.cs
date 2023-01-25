using Libria.Areas.Admin.Models;
using Libria.Areas.Admin.ViewModels.Products;
using Libria.Data;
using Libria.Models.Entities;
using Libria.Services;
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
		private readonly IServiceProvider _serviceProvider;

		public ProductsController(LibriaDbContext context, IServiceProvider serviceProvider)
		{
			_context = context;
			_serviceProvider = serviceProvider;
		}

		public async Task<IActionResult> Index(int category = -1, string? q = null, int page = 1)
		{
			int pageSize = 6;

			var query = _context.Books.AsNoTracking();

			if (category != -1)
				query = query.Where(b => b.Categories.Any(c => c.CategoryId == category));
			if (q != null)
				query = query.Where(b => EF.Functions.ILike(b.Title, $"%{q.Trim()}%"));

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

			var viewModel = new AllProductsViewModel(products, pageViewModel)
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
					PublisherId = b.PublisherId,
					Quantity = b.Quantity,
					PublicationYear = b.PublicationYear,
					Publisher = b.Publisher,
					Authors = b.Authors.Select(a => new Author { AuthorId = a.AuthorId }).ToList(),
					Categories = b.Categories.Select(c => new Category { CategoryId = c.CategoryId }).ToList()
				})
				.FirstOrDefaultAsync(b => b.BookId == id);

			if (book == null)
				return NotFound();

			// Select book's active relations
			var authorSelectItems = await GetAuthorSelectListItemsAsync(book.Authors);
			var categorySelectItems = await GetCategorySelectListItemsAsync(book.Categories);
			var publisherSelectItems = await GetPublisherSelectListItemsAsync(book.PublisherId);

			var viewModel = new EditProductViewModel()
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
				Quantity = book.Quantity,
				PublicationYear = book.PublicationYear,
				AuthorSelectItems = authorSelectItems,
				CategorySelectItems = categorySelectItems,
				PublisherSelectItems = publisherSelectItems,
			};

			return View(viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(EditProductViewModel? model)
		{
			if (model == null || model.BookId == null)
				return BadRequest();

			if (ModelState.IsValid)
			{
				var book = await _context.Books
					.Include(b => b.Authors)
					.Include(b => b.Categories)
					.FirstOrDefaultAsync(b => b.BookId == model.BookId);

				if (book == null)
					return NotFound();

				// keep original values to check if we need to send
				// notification about price drop or availability to users
				decimal originalPrice = book.SalePrice;
				bool originalAvailability = book.Available;

				// Update product image
				var fileSaveResult = await SaveProductImage(model.FileUpload, book.ImageUrl);

				switch (fileSaveResult.Status)
				{
					case FileSaveStatus.Saved:
						book.ImageUrl = fileSaveResult.FileUrl;
						break;
					case FileSaveStatus.EmptyFile:
					case FileSaveStatus.LargeFile:
					case FileSaveStatus.UnpermittedExtension:
						ModelState.AddModelError(nameof(model.FileUpload), fileSaveResult.ErrorMessage);
						model.AuthorSelectItems = await GetAuthorSelectListItemsAsync(model.SelectedAuthors.Select(id => new Author { AuthorId = id }).ToList());
						model.CategorySelectItems = await GetCategorySelectListItemsAsync(model.SelectedCategories.Select(id => new Category { CategoryId = id }).ToList());
                        model.PublisherSelectItems = await GetPublisherSelectListItemsAsync(model.SelectedPublisherId);
                        return View(model);
					case FileSaveStatus.Error:
						return Problem(fileSaveResult.ErrorMessage);
					default:
						return Problem("Сталася невідома помилка.");
				}

				// Update product authors and categories
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
				book.PublisherId = model.SelectedPublisherId;

				// Update properties
				book.Title = model.Title.Trim();
				book.Description = model.Description?.Trim();
				book.Price = model.Price;
				book.SalePrice = model.SalePrice == null || model.SalePrice > model.Price ? model.Price : (decimal)model.SalePrice;
				book.Pages = model.Pages;
				book.Quantity = model.Quantity;
				book.Available = model.Available;
				book.Isbn = model.Isbn?.Trim();
				book.PublicationYear = model.PublicationYear?.Trim();
				book.Language = model.Language?.Trim();

				try
				{
					await _context.SaveChangesAsync();

					if (book.Available)
					{
						// price drop notification
						if (book.SalePrice < originalPrice)
							_ = FireAndForgetNotificationSend(_serviceProvider, NotificationType.PriceDrop, book);
						// availability notification
						if (originalAvailability == false)
							_ = FireAndForgetNotificationSend(_serviceProvider, NotificationType.Availability, book);
					}
				}
				catch
				{
					return Problem("Something went wrong while writing to the database.");
				}

				return RedirectToAction("Edit");

			}

			model.AuthorSelectItems = await GetAuthorSelectListItemsAsync(model.SelectedAuthors.Select(id => new Author { AuthorId = id }).ToList());
			model.CategorySelectItems = await GetCategorySelectListItemsAsync(model.SelectedCategories.Select(id => new Category { CategoryId = id }).ToList());
            model.PublisherSelectItems = await GetPublisherSelectListItemsAsync(model.SelectedPublisherId);

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
			var publisherSelectItems = await _context.Publishers
				.Select(p => new SelectListItem { Text = p.Name, Value = p.PublisherId.ToString() })
				.ToListAsync();

			var viewModel = new EditProductViewModel
			{

				PageTitle = "Новий товар",
				ControllerName = "Products",
				ActionName = "Create",
				AuthorSelectItems = authorSelectItems,
				CategorySelectItems = categorySelectItems,
				PublisherSelectItems = publisherSelectItems
			};

			return View("Edit", viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> Create(EditProductViewModel? model)
		{
			if (model == null)
				return BadRequest();

			if (ModelState.IsValid)
			{
				Book book = new()
				{
					Title = model.Title.Trim(),
					Quantity = model.Quantity,
					Description = model.Description?.Trim(),
					Price = model.Price,
					SalePrice = model.SalePrice == null || model.SalePrice > model.Price ? model.Price : (decimal)model.SalePrice,
					Pages = model.Pages,
					Available = model.Available,
					Isbn = model.Isbn?.Trim(),
					PublisherId = model.SelectedPublisherId,
					PublicationYear = model.PublicationYear?.Trim(),
					Language = model.Language?.Trim(),
					Authors = model.SelectedAuthors.Select(id => new Author { AuthorId = id }).ToList(),
					Categories = model.SelectedCategories.Select(id => new Category { CategoryId = id }).ToList(),
					CreatedAt = DateTime.UtcNow
				};

				var fileSaveResult = await SaveProductImage(model.FileUpload, book.ImageUrl);

				switch (fileSaveResult.Status)
				{
					case FileSaveStatus.Saved:
						book.ImageUrl = fileSaveResult.FileUrl;
						break;
					case FileSaveStatus.EmptyFile:
					case FileSaveStatus.LargeFile:
					case FileSaveStatus.UnpermittedExtension:
						ModelState.AddModelError(nameof(model.FileUpload), fileSaveResult.ErrorMessage);
						ViewBag.FileUploadError = true;
						model.AuthorSelectItems = await GetAuthorSelectListItemsAsync(model.SelectedAuthors.Select(id => new Author { AuthorId = id }).ToList());
						model.CategorySelectItems = await GetCategorySelectListItemsAsync(model.SelectedCategories.Select(id => new Category { CategoryId = id }).ToList());
                        model.PublisherSelectItems = await GetPublisherSelectListItemsAsync(model.SelectedPublisherId);
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

			model.AuthorSelectItems = await GetAuthorSelectListItemsAsync(model.SelectedAuthors.Select(id => new Author { AuthorId = id }).ToList());
			model.CategorySelectItems = await GetCategorySelectListItemsAsync(model.SelectedCategories.Select(id => new Category { CategoryId = id }).ToList());
			model.PublisherSelectItems = await GetPublisherSelectListItemsAsync(model.SelectedPublisherId);

			return View("Edit", model);
		}

		public async Task<FileSaveResult> SaveProductImage(IFormFile? formFile, string? previousImg)
		{
			string[] permittedExtensions = { ".jpg", ".jpeg", ".png" };
			var result = new FileSaveResult();

			if (formFile != null)
			{	
				string ext = Path.GetExtension(formFile.FileName).ToLowerInvariant();
				if (!string.IsNullOrEmpty(ext) && permittedExtensions.Contains(ext))
				{
					if (formFile.Length > 0)
					{
						if (formFile.Length < 5_000_000) // Approx 5 MB
						{
							try
							{
								string fileName = DateTime.Now.ToString("yyyyMMddHHmm") + "_" + Path.GetRandomFileName() + ext;
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
					result.Status = FileSaveStatus.UnpermittedExtension;
					result.ErrorMessage = $"Лише файли *{string.Join("  *", permittedExtensions)}";
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

		private async Task<List<SelectListItem>> GetAuthorSelectListItemsAsync(IEnumerable<Author> selectedAuthors)
		{
			var listItems = await _context.Authors
				.Select(a => new SelectListItem
				{
					Text = a.Name,
					Value = a.AuthorId.ToString()
				})
				.ToListAsync();
			listItems.ForEach(i =>
			{
				if (selectedAuthors.Any(c => c.AuthorId == int.Parse(i.Value)))
					i.Selected = true;
			});
			return listItems;
		}

		private async Task<List<SelectListItem>> GetCategorySelectListItemsAsync(IEnumerable<Category> selectedCategories)
		{
			var listItems = await _context.Categories
				.Select(c => new SelectListItem { Text = c.Name, Value = c.CategoryId.ToString() })
				.ToListAsync();

			listItems.ForEach(i =>
			{
				if (selectedCategories.Any(c => c.CategoryId == int.Parse(i.Value)))
					i.Selected = true;
			});
			return listItems;
		}

		private async Task<List<SelectListItem>> GetPublisherSelectListItemsAsync(int? selectedPublisherId)
		{
			return await _context.Publishers
				.Select(p => new SelectListItem { Text = p.Name, Value = p.PublisherId.ToString(), Selected = p.PublisherId == selectedPublisherId })
				.ToListAsync();
		}

		private async Task FireAndForgetNotificationSend(IServiceProvider serviceProvider, NotificationType notificationType, Book book)
		{
			using (IServiceScope scope = serviceProvider.CreateScope())
			{
				var notificationService = scope.ServiceProvider.GetService<INotificationService>();
				if (notificationService == null)
					return;

				if (notificationType == NotificationType.PriceDrop)
					await notificationService.NotifyPriceDropAsync(book);
				else
					await notificationService.NotifyAvailableAsync(book);
			}
		}
	}
}
