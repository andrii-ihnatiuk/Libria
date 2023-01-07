using Libria.Areas.Admin.Models;

namespace Libria.Areas.Admin.ViewModels
{
	public class DashboardAuthorsViewModel
	{
		public DashboardAuthorsViewModel(List<AuthorCard> authorCards)
		{
			AuthorCards = authorCards;
		}

		public SidebarViewModel SidebarViewModel { get; set; } = new SidebarViewModel(MenuItemType.Authors);

		public List<AuthorCard> AuthorCards { get; set; }

		public string? CurrentSearchString { get; set; }
	}
}
