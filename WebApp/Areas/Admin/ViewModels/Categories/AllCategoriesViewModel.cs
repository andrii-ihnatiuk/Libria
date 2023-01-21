using Libria.Areas.Admin.Models;

namespace Libria.Areas.Admin.ViewModels.Categories
{
    public class AllCategoriesViewModel
    {
        public AllCategoriesViewModel(List<CategoryCard> categoryCards)
        {
            CategoryCards = categoryCards;
        }

        public SidebarViewModel SidebarViewModel { get; set; } = new SidebarViewModel(MenuItemType.Categories);

        public List<CategoryCard> CategoryCards { get; set; }

        public string? CurrentSearchString { get; set; }
    }
}
