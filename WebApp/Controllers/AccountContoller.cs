using Libria.Data;
using Libria.Models.Entities;
using Libria.Services;
using Libria.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Libria.Controllers
{
	public class AccountController : Controller
	{
		private readonly UserManager<User> _userManager;
		private readonly SignInManager<User> _signInManager;
		private readonly ILogger<AccountController> _logger;
		private readonly IEmailService _emailService;

		public AccountController(ILogger<AccountController> logger, UserManager<User> userManager, SignInManager<User> signInManager, IEmailService emailService)
		{
			_logger = logger;
			_userManager = userManager;
			_signInManager = signInManager;
			_emailService = emailService;
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
