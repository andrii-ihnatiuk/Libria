using Libria.Areas.Admin.Models;
using Libria.Data;
using Libria.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Libria.Areas.Admin.ViewModels
{
    public class DashboardOrdersViewModel
    {
        public DashboardOrdersViewModel(List<Order> orders)
        {
            Orders = orders;
        }

        public SidebarViewModel SidebarViewModel { get; set; } = new SidebarViewModel(MenuItemType.Orders);

        public List<SelectListItem> OrderStatusSelectItems { get; set; } = new List<SelectListItem>()
        {
            new SelectListItem { Text = "Усі", Value = OrderFilterOptions.All },
            new SelectListItem { Text = OrderStatus.Pending, Value = OrderFilterOptions.Pending },
            new SelectListItem { Text = OrderStatus.Sent, Value = OrderFilterOptions.Sent },
            new SelectListItem { Text = OrderStatus.Finished, Value = OrderFilterOptions.Finished }
        };

        public List<Order> Orders { get; set; }

        public string CurrentOrderStatusFilter { get; set; } = OrderFilterOptions.All;

        public DateTime? CurrentDateFilter { get; set; }
    }
}
