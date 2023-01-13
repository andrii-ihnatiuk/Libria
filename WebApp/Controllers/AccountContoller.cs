using Libria.Data;
using Libria.Models.Entities;
using Libria.Services;
using Libria.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Libria.Controllers
{
    public class AccountController : Controller
	{
		private readonly UserManager<User> _userManager;
		private readonly SignInManager<User> _signInManager;
		private readonly ILogger<AccountController> _logger;
		private readonly IEmailService _emailService;
		private readonly LibriaDbContext _context;

		public AccountController(ILogger<AccountController> logger, UserManager<User> userManager, SignInManager<User> signInManager, IEmailService emailService, LibriaDbContext context)
		{
			_logger = logger;
			_userManager = userManager;
			_signInManager = signInManager;
			_emailService = emailService;
			_context = context;
		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> Index()
		{
			var user = await _userManager.GetUserAsync(User);

			return View(user);
		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> OrderHistory(string show = "finished")
		{
			string? identifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (identifier == null)
				return Problem("User identifier not found");

			var query = _context.Orders.AsNoTracking().Where(o => o.UserId == identifier);

			if (show == "active")
			{
				query = query.Where(o => o.OrderStatus != OrderStatus.Finished && o.OrderStatus != OrderStatus.Canceled);
				ViewData["CurPill"] = show;
			}
			else
			{
				query = query.Where(o => o.OrderStatus == OrderStatus.Finished);
				ViewData["CurPill"] = "all";
			}

			var orders = await query
				.Select(o => new Order
				{
					OrderId = o.OrderId,
					Books = o.Books.Select(ob => new OrdersBooks
					{
						Price = ob.Price,
						Quantity = ob.Quantity,
						Book = new Book
						{
							BookId = ob.Book.BookId,
							Title = ob.Book.Title,
							ImageUrl = ob.Book.ImageUrl,
							Authors = ob.Book.Authors.Select(a => new Author { Name = a.Name }).ToList()
						}
					}).ToList(),
					OrderDate = o.OrderDate,
					TotalSpent = o.TotalSpent,
					FirstName = o.FirstName,
					LastName = o.LastName,
					PhoneNumber = o.PhoneNumber,
					Email = o.Email,
					OrderStatus = o.OrderStatus
				}).ToListAsync();

			//         var orders = new List<Order>()
			//         {
			//             new Order 
			//             { 
			//                 OrderId = 12321, 
			//                 OrderStatus = OrderStatus.Sent, 
			//                 TotalSpent = 1200, 
			//                 OrderDate = DateTime.UtcNow,
			//                 Books = new List<OrdersBooks> { new OrdersBooks { Quantity = 2, Price = 1200, Book = new Book { BookId = 5, ImageUrl = "/img/book_cover/5.jpg", Title = "Some book" } } }
			//             },
			//	new Order
			//	{
			//		OrderId = 4211,
			//		OrderStatus = OrderStatus.Pending,
			//		TotalSpent = 1000,
			//		OrderDate = DateTime.UtcNow,
			//		Books = new List<OrdersBooks> { new OrdersBooks { Quantity = 3, Price = 500, Book = new Book { BookId = 4, ImageUrl = "/img/book_cover/4.jpg", Title = "Another book" } } }
			//	}
			//};

			return View(orders);
		}

		// Registration

		[HttpGet]
		public IActionResult Register()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				User user = new()
				{
					UserName = model.Email,
					FirstName = model.FirstName,
					LastName = model.LastName ?? "",
					Email = model.Email,
					PhoneNumber = model.PhoneNumber
				};

				IdentityResult result = await _userManager.CreateAsync(user, model.Password);

				if (result.Succeeded)
				{
					await _signInManager.SignInAsync(user, isPersistent: false);
					return RedirectToAction("Index", "Home");
				}
				else
				{
					foreach (var error in result.Errors)
					{
						ModelState.AddModelError(string.Empty, error.Description);
					}
				}
			}
			return View(model);
		}

		// Authorization

		[HttpGet]
		public IActionResult Login(string? returnUrl)
		{
			if (returnUrl == null && string.IsNullOrEmpty(Request.Headers["Referer"]) == false)
			{
				var path = new Uri(Request.Headers["Referer"].ToString());
				ViewData["returnUrl"] = path.PathAndQuery.ToString();
			}
			else { ViewData["returnUrl"] = returnUrl; }
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
		{
			if (ModelState.IsValid)
			{
				var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

				if (result.Succeeded)
				{
					var user = await _userManager.FindByEmailAsync(model.Email);
					if (await _userManager.IsInRoleAsync(user, "admin"))
					{
						return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
					}
					if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
					{
						return LocalRedirect(returnUrl);
					}
					else
					{
						return RedirectToAction("Index", "Home");
					}
				}
				else
				{
					var user = await _userManager.FindByEmailAsync(model.Email);
					if (user == null)
					{
						ModelState.AddModelError(nameof(model.Email), ModelValidationMessages.Email);
						ViewBag.EmailInvalid = true;
					}
					else
					{
						ModelState.AddModelError(nameof(model.Password), ModelValidationMessages.PasswordIncorrect);
						ViewBag.PasswordInvalid = true;
					}
				}
			}
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}

		// Renew Access

		[HttpGet]
		public IActionResult ForgotPassword()
		{
			return View(new ForgotPasswordViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(model.Email);
				if (user != null)
				{
					_logger.LogInformation($"{HttpContext?.Request?.Scheme}; {HttpContext?.Request?.Protocol}");

					var token = await _userManager.GeneratePasswordResetTokenAsync(user);
					var callbackUrl = Url.Action(nameof(ResetPassword), "Account", new { uid = user.Id, token }, protocol: HttpContext?.Request.Scheme);

					var subject = "Запит на відновлення доступу";
					List<string> message = new() { $@"<p>Для зміни пароля перейдіть за</p><a href=""{callbackUrl}"">наступним посиланням</a>" };
					List<string> toEmail = new() { user.Email };
					var response = await _emailService.SendEmailAsync(toEmail, subject, message);

					if (response == EmailStatus.ErrorResult) return RedirectToAction("Error", "Home");
				}
			}
			model.RequestProcessed = true;
			return View(model);
		}

		[HttpGet]
		public IActionResult ResetPassword(string? uid = null, string? token = null)
		{
			return (uid == null || token == null) ? BadRequest() : View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByIdAsync(model.Uid);
				if (user == null)
				{
					return NotFound();
				}
				var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
				if (result.Succeeded)
				{
					ViewBag.PasswordChanged = true;
					return View();
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}
			else if (model.Password != model.ConfirmPassword)
			{
				ViewBag.PasswordMismatch = true;
			}
			return View(model);
		}

		public IActionResult AccessDenied()
		{
			return View();
		}
	}
}
