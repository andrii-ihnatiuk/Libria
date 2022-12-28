using Libria.Data;
using Libria.Models;

namespace Libria.Services
{
	public interface ISearchService
	{
		public Task<SearchResult?> SearchAsync(
			string? q = null, 
			int? categoryId = null, 
			int? authorId = null,
			string sortBy = SortState.Default,
			int page = 1,
			int pageSize = 3);
	}
}
