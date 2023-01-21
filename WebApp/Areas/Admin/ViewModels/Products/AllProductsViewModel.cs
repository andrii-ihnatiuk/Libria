using Libria.Areas.Admin.Models;
using Libria.Models.Entities;
using Libria.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Libria.Areas.Admin.ViewModels.Products
{
    public class AllProductsViewModel
    {
        public AllProductsViewModel(List<Book> products, PageViewModel pageViewModel)
        {
            Products = products;
            PageViewModel = pageViewModel;
        }

        public SidebarViewModel SidebarViewModel { get; set; } = new SidebarViewModel(MenuItemType.Products);

        public PageViewModel PageViewModel { get; set; }

        public List<SelectListItem> CategorySelectItems { get; set; } = null!;

        public string? CurrentSearchString { get; set; }

        private int _currentCategoryId;
        public int CurrentCategoryId
        {
            get => _currentCategoryId;
            set
            {
                var item = CategorySelectItems.FirstOrDefault(i => i.Value == value.ToString());
                if (item != null)
                    item.Selected = true;
                _currentCategoryId = value;
            }
        }

        public int TotalItemsFound { get; set; }

        public List<Book> Products { get; set; }
    }
}
