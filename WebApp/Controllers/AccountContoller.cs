using Libria.Data;
using Libria.Models.Entities;
using Libria.Services;
using Libria.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Principal;

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
		public async Task<IActionResult> Settings()
		{
			var user = await _userManager.GetUserAsync(User);

			//var user = new User { FirstName = "Andrii", LastName = "Ihnatiuk", Email = "email@example.com", PhoneNumber = "+380953234675" };

			SettingsViewModel viewModel = new() { Email = user.Email, FirstName = user.FirstName, LastName = user.LastName, PhoneNumber = user.PhoneNumber };
			return View(viewModel);
		}

		[Authorize]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public async Task<IActionResult> Settings(SettingsViewModel model)
		{
			if (model.UpdateField == null)
				return Problem("No update marker passed");

			RemoveUnusedModelErrors((UpdateField)model.UpdateField);

			var user = await _userManager.GetUserAsync(User);
			if (ModelState.IsValid)
			{
				switch (model.UpdateField)
				{
					case UpdateField.Name:
						user.FirstName = model.FirstName.Trim();
						user.LastName = model.LastName?.Trim() ?? string.Empty;
						break;
					case UpdateField.PhoneNumber:
						var number = model.PhoneNumber.Trim();
						if (user.PhoneNumber != number && await _context.Users.AnyAsync(u => u.PhoneNumber == number))
						{
							ModelState.AddModelError(string.Empty, "Дані не було збережено");
							ModelState.AddModelError(nameof(model.PhoneNumber), "Номер телефону вже зайнято");
							model.PhoneNumber = number;
							model.FirstName = user.FirstName;
							model.LastName = user.LastName;
							model.Email = user.Email;
							return View(model);
						}
						user.PhoneNumber = number;
						break;
					case UpdateField.Email:
						var newEmail = model.Email.Trim();
						if (user.Email != newEmail)
						{
							if (MailAddress.TryCreate(newEmail, out _))
							{
								IEnumerable<IdentityError>? errors = null;
								// set username first, because it is the same as email in my app,
								// but identity prevents username duplicates by default
								var emailRes = await _userManager.SetUserNameAsync(user, newEmail);
								if (emailRes.Succeeded)
								{
									var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
									emailRes = await _userManager.ChangeEmailAsync(user, newEmail, token);
									if (emailRes.Succeeded)
										await _signInManager.RefreshSignInAsync(user); // update identity sign-in cookie claims
									else
										errors = emailRes.Errors;
								}
								else
									errors = emailRes.Errors;

								if (errors != null && errors.Any())
								{
									ModelState.AddModelError(string.Empty, "Дані не було збережено");
									foreach (var err in errors)
										ModelState.AddModelError(nameof(model.Email), err.Description);
									model.PhoneNumber = user.PhoneNumber;
									model.FirstName = user.FirstName;
									model.LastName = user.LastName;
									return View(model);
								}
							}
							else
							{
								ModelState.AddModelError(string.Empty, "Дані не було збережено");
								ModelState.AddModelError(nameof(model.Email), "Введіть правильну email адресу");
								model.PhoneNumber = user.PhoneNumber;
								model.FirstName = user.FirstName;
								model.LastName = user.LastName;
								return View(model);
							}
						}
						break;
					case UpdateField.Password:
						var newPass = model.Password.Trim();
						var passRes = await _userManager.ChangePasswordAsync(user, model.OldPassword.Trim(), newPass);

						if (!passRes.Succeeded)
						{
							ModelState.AddModelError(string.Empty, "Дані не було збережено");
							foreach (var err in passRes.Errors)
							{
								if (err.Code == nameof(IdentityErrorDescriber.PasswordMismatch))
									ModelState.AddModelError(nameof(model.OldPassword), err.Description);
								else
									ModelState.AddModelError(nameof(model.Password), err.Description);
							}
							model.PhoneNumber = user.PhoneNumber;
							model.FirstName = user.FirstName;
							model.LastName = user.LastName;
							model.Email = user.Email;
							return View(model);
						}
						break;
				}
				await _context.SaveChangesAsync();
				return RedirectToAction("Settings");
			}
			ModelState.AddModelError(string.Empty, "Дані не було збережено");
			model.PhoneNumber = user.PhoneNumber;
			model.FirstName = user.FirstName;
			model.LastName = user.LastName;
			model.Email = user.Email;

			return View(model);
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
				query = query.Where(o => o.OrderStatus == OrderStatus.Finished || o.OrderStatus == OrderStatus.Canceled);
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
						ModelState.AddModelError(nameof(model.Email), ModelValidationMessages.Email);
					else
						ModelState.AddModelError(nameof(model.Password), ModelValidationMessages.PasswordIncorrect);
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

			return View(model);
		}

		public IActionResult AccessDenied()
		{
			return View();
		}

		private void RemoveUnusedModelErrors(UpdateField updateField)
		{
			List<string> fields = new()
			{
				nameof(SettingsViewModel.FirstName),
				nameof(SettingsViewModel.LastName),
				nameof(SettingsViewModel.PhoneNumber),
				nameof(SettingsViewModel.Email),
				nameof(SettingsViewModel.Password),
				nameof(SettingsViewModel.OldPassword)
			};

			switch (updateField)
			{
				case UpdateField.Name:
					fields.Remove(nameof(SettingsViewModel.FirstName));
					fields.Remove(nameof(SettingsViewModel.LastName));
					break;
				case UpdateField.Email:
					fields.Remove(nameof(SettingsViewModel.Email));
					break;
				case UpdateField.Password:
					fields.Remove(nameof(SettingsViewModel.Password));
					fields.Remove(nameof(SettingsViewModel.OldPassword));
					break;
				case UpdateField.PhoneNumber:
					fields.Remove(nameof(SettingsViewModel.PhoneNumber));
					break;
			}

			foreach (var field in fields)
				ModelState.Remove(field);
		}
	}
}
