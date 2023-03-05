using Libria.Data;
using Libria.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Libria.Services
{
	public class NotificationService : INotificationService
	{
		private readonly LibriaDbContext _context;
		private readonly IEmailService _emailService;

		public NotificationService(LibriaDbContext context, IEmailService emailService)
		{
			_context = context;
			_emailService = emailService;
		}

		public async Task<NotificationRegisterStatus> SubscribeForNotificationAsync(string userEmail, int bookId, NotificationType type)
		{
			if (string.IsNullOrEmpty(userEmail))
				return NotificationRegisterStatus.Failed;
			var book = await _context.Books.Select(b => new Book { BookId = b.BookId }).FirstOrDefaultAsync(b => b.BookId == bookId);
			if (book == null)
				return NotificationRegisterStatus.Failed;
			else if (_context.Notifications.Any(n => n.UserEmail == userEmail && n.TargetBookId == bookId && n.NotificationType == type))
				return NotificationRegisterStatus.RegisteredPreviously;

			try
			{
				_context.Notifications.Add(new Notification { UserEmail = userEmail, TargetBookId = bookId, NotificationType = type });
				await _context.SaveChangesAsync();
				return NotificationRegisterStatus.Ok;
			}
			catch
			{
				return NotificationRegisterStatus.Failed;
			}
		}

		public NotificationRegisterStatus Unsubscribe(int subscriptionId, string userEmail, int bookId)
		{
			var res = _context.Database
				.ExecuteSqlInterpolated($@"DELETE FROM ""Notifications"" WHERE ""Id"" = {subscriptionId} AND ""UserEmail"" = {userEmail} AND ""TargetBookId"" = {bookId}");
			return res == 0 ? NotificationRegisterStatus.Failed : NotificationRegisterStatus.Ok;
		}

		public async Task<int> NotifyPriceDropAsync(Book book)
		{
			var notifications = await _context.Notifications
				.Where(n => n.TargetBookId == book.BookId && n.NotificationType == NotificationType.PriceDrop)
				.Select(n => new Notification { UserEmail =  n.UserEmail, Id = n.Id }).ToListAsync();

			if (notifications.Count == 0)
				return 1;

			List<string> emailGroup = notifications.Select(n => n.UserEmail).ToList();
			List<string> messages = new();
			string subject = "Зниження вартості товару";

			foreach (var n in notifications)
			{
				string message = @$"
				<!DOCTYPE html>
				<html>
				<head>
				</head>
				<body>
					<h3>Вітаємо, повідомляємо Вас про зниження вартості товару.</h3>
					<div style='margin-top:1rem;margin-bottom:1rem;'>
						<div>Книжка «{book.Title}» тепер доступна за ціною {book.SalePrice} грн. замість {book.Price} грн.</div>
						<a href='https://libria.pp.ua/Book?bookId={book.BookId}'>Переходьте</a>
						<span> та купуйте вже зараз!</span>
					</div>
					<div>Libria - 2023</div>
					<a href='https://libria.pp.ua/Book/UnsubscribeNotification?subscriptionId={n.Id}&userEmail={n.UserEmail}&bookId={book.BookId}'>Відписатися від повідомлення</a>
				</body>
				</html>
				";
				messages.Add(message);
			}

			EmailStatus result = await _emailService.SendEmailAsync(emailGroup, subject, messages);

			return result == EmailStatus.SuccessResult ? 1 : 0;
		}

		public async Task<int> NotifyAvailableAsync(Book book)
		{
			var notifications = await _context.Notifications
				.Where(n => n.TargetBookId == book.BookId && n.NotificationType == NotificationType.Availability)
				.Select(n => new Notification { UserEmail = n.UserEmail, Id = n.Id }).ToListAsync();

			if (notifications.Count == 0)
				return 1;

			List<string> emailGroup = notifications.Select(n => n.UserEmail).ToList();
			List<string> messages = new();
			string subject = "Наявність товару";
			
			foreach (var n in notifications)
			{
				string message = @$"
				<!DOCTYPE html>
				<html>
				<head>
				</head>
				<body>
					<h3>Вітаємо, повідомляємо Вас про наявність товару.</h3>
					<p></p>
					<div style='margin-top:1rem;margin-bottom:1rem;'>
						<div>Книжка «{book.Title}» тепер в наявності у нашому магазині за ціною {book.SalePrice} грн.</div>
						<a href='https://libria.pp.ua/Book?bookId={book.BookId}'>Переходьте</a>
						<span> та купуйте вже зараз!</span>
					</div>
					<div>Libria - 2023</div>
					<a href='https://libria.pp.ua/Book/UnsubscribeNotification?subscriptionId={n.Id}&userEmail={n.UserEmail}&bookId={book.BookId}'>Відписатися від повідомлення</a>
				</body>
				</html>
				";
				messages.Add(message);
			}

			EmailStatus result = await _emailService.SendEmailAsync(emailGroup, subject, messages);

			return result == EmailStatus.SuccessResult ? 1 : 0;
		}

	}
}
