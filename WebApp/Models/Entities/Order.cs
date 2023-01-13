namespace Libria.Models.Entities
{
	public class Order
	{
		public Order()
		{
			Books = new List<OrdersBooks>();
		}

		public int OrderId { get; set; }
		public string? UserId { get; set; }

		public string FirstName { get; set; } = null!;
		public string LastName { get; set; } = null!;
		public string PhoneNumber { get; set; } = null!;
		public string Email { get; set; } = null!;

		public DateTime OrderDate { get; set; }
		public decimal TotalSpent { get; set; }
		public string OrderStatus { get; set; } = Data.OrderStatus.Pending;

		public ICollection<OrdersBooks> Books { get; set; } = null!;

		public string GetFormattedDate() => OrderDate.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss");
	}
}
