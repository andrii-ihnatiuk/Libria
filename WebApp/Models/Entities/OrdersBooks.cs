namespace Libria.Models.Entities
{
	public class OrdersBooks
	{
		public int OrderId { get; set; }
		public int BookId { get; set; }
		public decimal Price { get; set; }
		public int Quantity { get; set; }

		public Order Order { get; set; } = null!;
		public Book Book { get; set; } = null!;
	}
}
