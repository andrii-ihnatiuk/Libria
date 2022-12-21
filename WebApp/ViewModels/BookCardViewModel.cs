using Libria.Models.Entities;

namespace Libria.ViewModels
{
    public class BookCardViewModel
	{
		public Book Book { get; set; } = null!;
		public bool Wished { get; set; }
	}
}
