using Libria.Models;

namespace Libria.ViewModels
{
	public class CardDisplayViewModel
	{
		public Book Book { get; set; } = null!;

		public bool Wished { get; set; }
	}
}
