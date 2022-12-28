namespace Libria.ViewModels
{
	public class FilterViewModel
	{
		public string? SearchString { get; set; } = null;

		public int? CategoryId { get; set; } = null;

		public int? AuthorId { get; set; } = null;
	}
}