namespace Libria.Models
{
	public class WishList
	{
		public string UserId { get; set; } = null!;
		public int BookId { get; set; }
		
		public User User { get; set; } = null!;
		public Book Book { get; set; } = null!;
	}
}