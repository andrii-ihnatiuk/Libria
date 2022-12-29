using Libria.Areas.Admin.Models;
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
                .Where(o => o.OrderStatus == OrderStatuses.Processing)
                .Select(o => new Order { TotalSpent = o.TotalSpent, OrderStatus = o.OrderStatus })
                .ToListAsync();
            ViewData["newOrdersCount"] = newOrders.Count;
            ViewData["newOrdersPrice"] = newOrders.Sum(o => o.TotalSpent);

            var viewModel = new DashboardViewModel
            {
                ActiveMenuItem = MenuItemNames.Main
            };
            return View(viewModel);
        }

        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders.AsNoTracking()
                .Select(o => new Order
                {
                    OrderDate = o.OrderDate,
                    OrderStatus = o.OrderStatus,
                    TotalSpent = o.TotalSpent,
                    FirstName = o.FirstName,
                    LastName = o.LastName,
                }).ToListAsync();

            var viewModel = new DashboardViewModel
            {
                ActiveMenuItem = MenuItemNames.Orders
            };
            return View(viewModel);
        }
    }
}
