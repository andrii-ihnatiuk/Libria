using Libria.ViewModels;

namespace Libria.Models
{
	public class SearchResult
	{
		public int ResultsCount { get; set; }

		public List<BookCardViewModel> Data { get; set; } = null!;
	}
}
