using Libria.Areas.Admin.Models;
using Libria.Areas.Admin.ViewModels;
using Libria.Data;
using Libria.Models.Entities;
using Libria.ViewModels;
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

        public async Task<IActionResult> Orders(DateTime? date, string show = OrderFilterOptions.All, int page = 1)
        {
            var query = _context.Orders.AsNoTracking();
            var pageSize = 2; // just for testing

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

            var count = await query.CountAsync();

            var pageViewModel = new PageViewModel(count, page, pageSize);

            // In case there is 0 results just output
			if (page > pageViewModel.TotalPages && pageViewModel.TotalPages != 0 || page < 1)
				return NotFound();

            List<Order> orders;
            if (pageViewModel.TotalPages > 0)
            {
			    orders = await query
                    .OrderByDescending(o => o.OrderId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            else
            {
                orders = new();
            }

            var viewModel = new DashboardOrdersViewModel(orders, pageViewModel)
            {
                CurrentDateFilter = date?.ToString("yyy-MM-dd"),
				CurrentOrderStatusFilter = show,
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Categories(string? q)
        {
            var categories = await _context.Categories.ToListAsync();
            var viewModel = new DashboardCategoriesViewModel(categories);

            return View(viewModel);
        }
    }
}
