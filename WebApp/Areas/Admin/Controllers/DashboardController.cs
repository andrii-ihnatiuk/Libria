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

        public async Task<IActionResult> Orders(DateTime? date, string show = OrderFilterOptions.All, int page = 1)
        {
            var query = _context.Orders.AsNoTracking();
            var pageSize = 2; // just for testing

            switch (show)
            {
                case OrderFilterOptions.Pending:
                    query = query.Where(o => o.OrderStatus == OrderStatus.Pending);
                    break;
				case OrderFilterOptions.Confirmed:
					query = query.Where(o => o.OrderStatus == OrderStatus.Confirmed);
					break;
				case OrderFilterOptions.Sent:
					query = query.Where(o => o.OrderStatus == OrderStatus.Sent); 
                    break;
                case OrderFilterOptions.Finished:
					query = query.Where(o => o.OrderStatus == OrderStatus.Finished);
					break;
                case OrderFilterOptions.Canceled:
					query = query.Where(o => o.OrderStatus == OrderStatus.Canceled);
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

        [HttpGet]
        public async Task<IActionResult> Order(int? id = null)
        {
            if (id == null)
                return NotFound();

			var order = await _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderId == id).FirstOrDefaultAsync();

            if (order == null)
                return NotFound();

            var orderBooks = await _context.Books
                .Join(_context.OrdersBooks.Where(o => o.OrderId == order.OrderId),
                b => b.BookId,
                o => o.BookId,
                (b, o) => new CartItemViewModel
                {
                    Book = new Book
                    {
                        BookId = b.BookId,
                        Title = b.Title,
                        ImageUrl = b.ImageUrl,
                        Authors = b.Authors.Select(a => new Author { Name = a.Name }).ToList(),
                    },
                    ActiveBookPrice = o.Price,
                    Quantity = o.Quantity,
                    TotalItemPrice = o.Price * o.Quantity
                }).ToListAsync();

            var viewModel = new DashboardOrderDetailsViewModel
            {
                Order = order,
                OrderItems = orderBooks,
                CurrentOrderStatus = order.OrderStatus
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Order(string? status, int? id = null)
        {
            if (status != OrderStatus.Pending && status != OrderStatus.Confirmed 
                && status != OrderStatus.Sent && status != OrderStatus.Finished 
                && status != OrderStatus.Canceled || id == null)
            {
                return BadRequest();
            }

            var order = await _context.Orders
                .Where(o => o.OrderId == id)

                .FirstOrDefaultAsync();
            if (order == null)
                return NotFound();

            order.OrderStatus = status;
            await _context.SaveChangesAsync();

            return RedirectToAction("Order");
        }

        public async Task<IActionResult> Categories(string? q)
        {
            var categories = await _context.Categories.ToListAsync();
            var viewModel = new DashboardCategoriesViewModel(categories);

            return View(viewModel);
        }
    }
}
