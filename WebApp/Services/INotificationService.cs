using Libria.Data;

namespace Libria.Services
{
    public interface INotificationService
	{
		public Task<NotificationRegisterStatus> SubscribeForNotificationAsync(string userEmail, int bookId, NotificationType type);

		public NotificationRegisterStatus Unsubscribe(int subscriptionId, string userEmail, int bookId);
	}
}
