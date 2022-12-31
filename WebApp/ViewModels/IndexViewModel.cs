using Libria.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Libria.ViewModels
{
	public class IndexViewModel
	{
		public List<BookCardViewModel> BookCards { get; set; } = null!;

		public PageViewModel PageViewModel { get; set; } = null!;

		public FilterViewModel FilterViewModel { get; set; } = null!;

		public string ControllerName { get; set; } = null!;

		public string ActionName { get; set; } = null!;

		public List<SelectListItem> SelectListItems { get; set; } = new List<SelectListItem>
		{
			new SelectListItem { Text = "За замовчуванням", Value = SortState.Default },
			new SelectListItem { Text = "Від А до Я", Value = SortState.NameAsc },
			new SelectListItem { Text = "Від Я до А", Value = SortState.NameDesc },
			new SelectListItem { Text = "Від найдорожчих", Value = SortState.PriceDesc },
			new SelectListItem { Text = "Від найдешевших", Value = SortState.PriceAsc },
		};

		private string _currentSortState = SortState.Default;
		public string CurrentSortState 
		{
			get => _currentSortState;
			set
			{
				var selectItem = SelectListItems.Find(i => i.Value == value);
				if (selectItem != null)
					selectItem.Selected = true;
				_currentSortState = value;
			}
		}
	}
}
