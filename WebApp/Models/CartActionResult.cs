namespace Libria.Models
{
	public class CartActionResult
	{
		public bool Success { get; set; }
		public string? ErrorMessage { get; set; }
		public int? BookId { get; set; }
		public int? NewQuantity { get; set; }
		public decimal? TotalItemPrice { get; set; }
		public decimal? TotalCartPrice { get; set; }
	}
}
