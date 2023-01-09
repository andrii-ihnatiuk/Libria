using Libria.Data;
using Libria.Models.Entities;

namespace Libria.Services
{
    public interface INotificationService
	{
		public Task<NotificationRegisterStatus> SubscribeForNotificationAsync(string userEmail, int bookId, NotificationType type);

		public NotificationRegisterStatus Unsubscribe(int subscriptionId, string userEmail, int bookId);

		public Task<int> NotifyPriceDropAsync(Book book);

		public Task<int> NotifyAvailableAsync(Book book);
	}
}
