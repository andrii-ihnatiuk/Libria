namespace Libria.Models.Entities
{
	public class Order
	{
		public Order()
		{
			Books = new List<OrdersBooks>();
		}

		public int OrderId { get; set; }
		public string UserId { get; set; } = null!;
		public DateTime OrderDate { get; set; }
		public decimal TotalSpent { get; set; }
		public string? OrderStatus { get; set; }

		public ICollection<OrdersBooks> Books { get; set; } = null!;
	}
}
