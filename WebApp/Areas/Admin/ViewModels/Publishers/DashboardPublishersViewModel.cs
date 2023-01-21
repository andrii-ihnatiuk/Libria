using Libria.Areas.Admin.Models;

namespace Libria.Areas.Admin.ViewModels.Publishers
{
    public class DashboardPublishersViewModel
    {
        public DashboardPublishersViewModel(List<PublisherCard> publisherCards)
        {
            PublisherCards = publisherCards;
        }

        public SidebarViewModel SidebarViewModel { get; set; } = new SidebarViewModel(MenuItemType.Publishers);

        public List<PublisherCard> PublisherCards { get; set; }

        public string? CurrentSearchString { get; set; }
    }
}
