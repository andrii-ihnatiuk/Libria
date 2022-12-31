using Libria.Areas.Admin.Models;
using Libria.Models.Entities;

namespace Libria.Areas.Admin.ViewModels
{
	public class DashboardCategoriesViewModel
	{
		public DashboardCategoriesViewModel(List<Category> categories)
		{ 
			Categories = categories;
		}

		public SidebarViewModel SidebarViewModel { get; set; } = new SidebarViewModel(MenuItemType.Categories);

		public List<Category> Categories { get; set; }
	}
}
