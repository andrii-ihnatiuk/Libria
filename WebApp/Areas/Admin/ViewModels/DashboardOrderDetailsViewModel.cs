using Libria.Areas.Admin.Models;
using Libria.Data;
using Libria.Models.Entities;
using Libria.ViewModels.Cart;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Libria.Areas.Admin.ViewModels
{
	public class DashboardOrderDetailsViewModel
	{
		public Order Order { get; set; } = null!;

		public List<CartItemViewModel> OrderItems { get; set; } = null!;

		public List<SelectListItem> OrderStatusSelectItems { get; set; } = new List<SelectListItem>()
		{
			new SelectListItem { Text = OrderStatus.Pending, Value = OrderStatus.Pending },
			new SelectListItem { Text = OrderStatus.Confirmed, Value = OrderStatus.Confirmed },
			new SelectListItem { Text = OrderStatus.Sent, Value = OrderStatus.Sent },
			new SelectListItem { Text = OrderStatus.Finished, Value = OrderStatus.Finished },
			new SelectListItem { Text = OrderStatus.Canceled, Value = OrderStatus.Canceled }
		};

		private string _currentOrderStatus = OrderFilterOptions.Pending;
		public string CurrentOrderStatus
		{
			get => _currentOrderStatus;
			set
			{
				var item = OrderStatusSelectItems.Find(s => s.Value == value);
				if (item != null)
					item.Selected = true;
				_currentOrderStatus = value;
			}
		}
	}
}
