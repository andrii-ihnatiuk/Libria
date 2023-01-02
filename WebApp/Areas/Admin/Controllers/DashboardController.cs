using Libria.Areas.Admin.Models;
using Libria.Areas.Admin.ViewModels;
using Libria.Data;
using Libria.Models.Entities;
using Libria.ViewModels;
using Libria.ViewModels.Cart;
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
    }
}
