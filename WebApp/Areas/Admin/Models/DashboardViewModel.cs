namespace Libria.Areas.Admin.Models
{
	public class DashboardViewModel
	{
		public List<SidebarMenuItem> SidebarMenuItems { get; set; } = new List<SidebarMenuItem>()
		{
			new SidebarMenuItem
			{
                ActionName = "Index",
				Name = MenuItemNames.Main
			},
			new SidebarMenuItem
			{
                ActionName = "Orders",
                Name = MenuItemNames.Orders
            },
			new SidebarMenuItem
			{
				ActionName = "Categories",
                Name = MenuItemNames.Categories
            },
			new SidebarMenuItem
			{
                ActionName = "Products",
				Name = MenuItemNames.Products
            }
		};

		public string ActiveMenuItem { get; set; } = "Index";
	}
}
