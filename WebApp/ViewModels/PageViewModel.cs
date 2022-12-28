namespace Libria.ViewModels
{
	public class PageViewModel
	{
		public int PageNumber { get; set; }
		public int TotalPages { get; set; }
		public int ItemsCount { get; set; }

		public PageViewModel(int itemsCount, int pageNumber, int pageSize = 3) 
		{
			ItemsCount = itemsCount;
			PageNumber = pageNumber;
			TotalPages = (int)Math.Ceiling((double)itemsCount / pageSize);
		}

		public bool HasPreviousPage
		{
			get
			{
				return PageNumber > 1;
			}
		}

		public bool HasNextPage
		{
			get
			{
				return PageNumber < TotalPages;
			}
		}
	}
}
