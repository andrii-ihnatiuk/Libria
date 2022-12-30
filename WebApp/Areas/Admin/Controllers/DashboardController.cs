using Libria.Areas.Admin.Models;
using Libria.Areas.Admin.ViewModels;
using Libria.Data;
using Libria.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Libria.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class DashboardController : Controller
    {
        private readonly LibriaDbContext _context;

        public DashboardController(LibriaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["usersCount"] = await _context.Users.CountAsync();
            var newOrders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderStatus == OrderStatus.Pending)
                .Select(o => new Order { TotalSpent = o.TotalSpent, OrderStatus = o.OrderStatus })
                .ToListAsync();
            ViewData["newOrdersCount"] = newOrders.Count;
            ViewData["newOrdersPrice"] = newOrders.Sum(o => o.TotalSpent);

            var viewModel = new SidebarViewModel(MenuItemType.Main);

            return View(viewModel);
        }

        public async Task<IActionResult> Orders(DateTime? date, string show = OrderFilterOptions.All)
        {
            var query = _context.Orders.AsNoTracking();

            switch (show)
            {
                case OrderFilterOptions.Pending:
                    query = query.Where(o => o.OrderStatus == OrderStatus.Pending);
                    break;
                case OrderFilterOptions.Sent:
					query = query.Where(o => o.OrderStatus == OrderStatus.Sent); 
                    break;
                case OrderFilterOptions.Finished:
					query = query.Where(o => o.OrderStatus == OrderStatus.Finished);
					break;
                default:
                    break;
            }

            if (date != null)
            {
                date = DateTime.SpecifyKind(date.Value, DateTimeKind.Utc);
                query = query.Where(o => o.OrderDate.Date == date.Value.Date);
            }

            var orders = await query.ToListAsync();

            var viewModel = new DashboardOrdersViewModel(orders)
            {
                Orders = orders,
                CurrentDateFilter = date,
                CurrentOrderStatusFilter = show
            };
            var item = viewModel.OrderStatusSelectItems.Find(i => i.Value == show);
            if (item != null)
                item.Selected = true;

            return View(viewModel);
        }
    }
}
