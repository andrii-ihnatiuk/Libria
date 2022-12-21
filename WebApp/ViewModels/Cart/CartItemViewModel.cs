using Libria.Models.Entities;

namespace Libria.ViewModels.Cart
{
    public class CartItemViewModel
	{
		public Book Book { get; set; } = null!;
		public int Quantity { get; set; } = 1;
		public decimal FinalPrice { get; set; }
	}
}
