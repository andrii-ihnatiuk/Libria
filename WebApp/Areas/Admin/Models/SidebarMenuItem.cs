namespace Libria.Areas.Admin.Models
{
    public class SidebarMenuItem
	{
		public MenuItemType ItemType { get; set; }

		public string ControllerName { get; set; } = "DashBoard";
		
		public string ActionName { get; set; } = "Index";

		public string Name { get; set; } = "Головна";
	}
}
