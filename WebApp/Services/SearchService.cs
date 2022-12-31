using Libria.Data;
using Libria.Models;
using Libria.Models.Entities;
using Libria.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Libria.Services
{
	public class SearchService : ISearchService
	{
		private readonly LibriaDbContext _context;
		private readonly ILogger<SearchService> _logger;

		public SearchService(LibriaDbContext context, ILogger<SearchService> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task<SearchResult?> SearchAsync(
			string? q = null, 
			int? categoryId = null, 
			int? authorId = null,
			string sortBy = SortState.Default,
			int page = 1,
			int pageSize = 3)
		{
			IQueryable<Book>? searchResult = null;

			/* FILTER BY KEY WORD */
			if (string.IsNullOrEmpty(q) == false)
			{
				// search books by book title search string
				var booksSearch = _context.Books
					.AsNoTracking()
					.Where(b => EF.Functions.ILike(b.Title, $"%{q}%")) // PostgreSql extension for case insensitive LIKE
					.Select(b => new Book { BookId = b.BookId, Title = b.Title, SalePrice = b.SalePrice });
				// search books by author name search string
				var authorsSearch = _context.Authors
					.AsNoTracking()
					.Where(a => EF.Functions.ILike(a.Name, $"%{q}%"))
					.SelectMany(a => a.Books.Select(b => new Book { BookId = b.BookId, Title = b.Title, SalePrice = b.SalePrice }));

				// distinct join two results
				searchResult = booksSearch.Union(authorsSearch);
			}
			/* FILTER BY CATEGORY */
			if (categoryId != null)
			{
				var booksByCategory = _context.Books
					.AsNoTracking()
					.Where(b => b.Categories.Any(c => c.CategoryId == categoryId))
					.Select(b => new Book { BookId = b.BookId, Title = b.Title, SalePrice = b.SalePrice });

				searchResult = searchResult == null ? booksByCategory : booksByCategory.Union(searchResult);
			}
			/* FILTER BY AUTHOR */
			if (authorId != null)
			{
				var booksByAuthor = _context.Authors
					.AsNoTracking()
					.Where(a => a.AuthorId == authorId)
					.SelectMany(a => a.Books.Select(b => new Book { BookId = b.BookId, Title = b.Title, SalePrice = b.SalePrice }));

				searchResult = searchResult == null ? booksByAuthor : booksByAuthor.Union(searchResult);
			}

			if (searchResult == null)
				return null;

			// pagination
			var count = await searchResult.CountAsync();

			var totalPages = (int)Math.Ceiling((double)count / pageSize);

			if (page > totalPages && totalPages != 0 || page < 1)
				return null;

			List<BookCardViewModel> pageItems;
			if (totalPages > 0)
			{
				searchResult = SetColumnToOrderBy(searchResult, sortBy);
				// Select book ids first and then load authors relation in separate query
				// I do this because EF does not correctly translate .Include / ignore it at all
				// when used with .Union and i haven't found any workaround for this problem.
				var itemsIds = await searchResult
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.Select(b => b.BookId)
					.ToListAsync();

				searchResult = _context.Books
					.AsNoTracking()
					.Where(b => itemsIds.Contains(b.BookId));
				searchResult = SetColumnToOrderBy(searchResult, sortBy);

				pageItems = await searchResult
					.Select(b => new BookCardViewModel 
					{ 
						Book = new Book 
						{
							BookId = b.BookId,
							Title = b.Title,
							Available = b.Available,
							Price = b.Price,
							SalePrice = b.SalePrice,
							Authors = b.Authors.Select(a => new Author { Name = a.Name }).ToList(),
							ImageUrl = b.ImageUrl
						}, 
						Wished = false })
					.ToListAsync();
			}
			else // if we we haven't found anything there is no sense to disturb DB for data
			{
				pageItems = new List<BookCardViewModel>();
			}

			return new SearchResult { Data = pageItems, ResultsCount = count };
		}

		private IQueryable<Book> SetColumnToOrderBy(IQueryable<Book> searchResult, string sortBy)
		{
			return sortBy switch
			{
				SortState.NameAsc => searchResult.OrderBy(s => s.Title),
				SortState.NameDesc => searchResult.OrderByDescending(s => s.Title),
				SortState.PriceAsc => searchResult.OrderBy(s => s.SalePrice),
				SortState.PriceDesc => searchResult.OrderByDescending(s => s.SalePrice),
				_ => searchResult,
			};
		}
	}
}
