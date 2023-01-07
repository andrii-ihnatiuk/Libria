using Libria.Data;
using Libria.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Libria.Services
{
    public class NotificationService : INotificationService
	{
		private readonly LibriaDbContext _context;

		public NotificationService(LibriaDbContext context)
		{
			_context = context;
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
	}
}
