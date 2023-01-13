using Libria.Areas.Admin.Models;

namespace Libria.Areas.Admin.ViewModels
{
    public class SidebarViewModel
	{
		public SidebarViewModel() { }

		public SidebarViewModel(MenuItemType activeItem)
		{
			ActiveMenuItem = activeItem;
		}

		public List<SidebarMenuItem> SidebarMenuItems { get; set; } = new List<SidebarMenuItem>()
		{
			new SidebarMenuItem
			{
				ItemType = MenuItemType.Main,
				ActionName = "Index",
				Name = "Головна"
			},
			new SidebarMenuItem
			{
				ItemType = MenuItemType.Orders,
				ControllerName = "Orders",
				ActionName = "Index",
				Name = "Замовлення"
			},
			new SidebarMenuItem
			{
				ItemType = MenuItemType.Categories,
				ControllerName = "Categories",
				ActionName = "Index",
				Name = "Категорії"
			},
			new SidebarMenuItem
			{
				ItemType = MenuItemType.Authors,
				ControllerName = "Authors",
				ActionName = "Index",
				Name = "Автори"
			},
			new SidebarMenuItem
			{
				ItemType = MenuItemType.Products,
				ControllerName = "Products",
				ActionName = "Index",
				Name = "Товари"
			}
		};

		public MenuItemType ActiveMenuItem { get; set; } = MenuItemType.Main;
	}
}
