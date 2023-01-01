using Libria.Areas.Admin.Models;
using Libria.Data;
using Libria.Models.Entities;
using Libria.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Libria.Areas.Admin.ViewModels
{
    public class DashboardOrdersViewModel
    {
        public DashboardOrdersViewModel(List<Order> orders, PageViewModel pageViewModel)
        {
            Orders = orders;
            PageViewModel = pageViewModel;
		}

        public SidebarViewModel SidebarViewModel { get; set; } = new SidebarViewModel(MenuItemType.Orders);

        public List<SelectListItem> OrderStatusSelectItems { get; set; } = new List<SelectListItem>()
        {
            new SelectListItem { Text = "Усі", Value = OrderFilterOptions.All },
            new SelectListItem { Text = OrderStatus.Pending, Value = OrderFilterOptions.Pending },
            new SelectListItem { Text = OrderStatus.Confirmed, Value = OrderFilterOptions.Confirmed },
            new SelectListItem { Text = OrderStatus.Sent, Value = OrderFilterOptions.Sent },
            new SelectListItem { Text = OrderStatus.Finished, Value = OrderFilterOptions.Finished },
            new SelectListItem { Text = OrderStatus.Canceled, Value = OrderFilterOptions.Canceled }
        };

        private string _currentOrderStatusFilter = OrderFilterOptions.All;
        public string CurrentOrderStatusFilter 
        {
            get => _currentOrderStatusFilter;
            set
            {
				var item = OrderStatusSelectItems.Find(i => i.Value == value);
				if (item != null)
					item.Selected = true;
				_currentOrderStatusFilter = value;
			}
        }

        public List<Order> Orders { get; set; }

        public PageViewModel PageViewModel { get; set; }

        public string? CurrentDateFilter { get; set; }
    }
}
