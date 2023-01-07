using Libria.Data;

namespace Libria.Models.Entities
{
    public class Notification
	{
		public int Id { get; set; }
		public string UserEmail { get; set; } = null!;
		public NotificationType NotificationType { get; set; }

		public int TargetBookId { get; set; }
		public Book TargetBook { get; set; } = null!;
	}
}
