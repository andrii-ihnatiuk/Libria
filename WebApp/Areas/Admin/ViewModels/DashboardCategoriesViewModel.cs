using Libria.Areas.Admin.Models;

namespace Libria.Areas.Admin.ViewModels
{
    public class DashboardCategoriesViewModel
	{
		public DashboardCategoriesViewModel(List<CategoryCard> categoryCards)
		{
			CategoryCards = categoryCards;
		}

		public SidebarViewModel SidebarViewModel { get; set; } = new SidebarViewModel(MenuItemType.Categories);

		public List<CategoryCard> CategoryCards { get; set; }

		public string? CurrentSearchString { get; set; }
	}
}
