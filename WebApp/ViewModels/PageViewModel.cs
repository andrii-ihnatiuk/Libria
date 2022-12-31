namespace Libria.ViewModels
{
	public class PageViewModel
	{
		public int PageNumber { get; set; }
		public int TotalPages { get; set; }
		public int ItemsCount { get; set; }

		public int AppendLeft { get; set; }
		public int AppendRight { get; set; }

		public PageViewModel(int itemsCount, int pageNumber, int pageSize = 3) 
		{
			ItemsCount = itemsCount;
			PageNumber = pageNumber;
			TotalPages = (int)Math.Ceiling((double)itemsCount / pageSize);

			if (PageNumber - 1 < 2)
				AppendRight = 3 - PageNumber;
			else if ((TotalPages - PageNumber) < 2)
				AppendLeft = 2 - (TotalPages - PageNumber);
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
