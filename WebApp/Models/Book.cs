using System.ComponentModel.DataAnnotations;

namespace Libria.Models
{
	public class Book
	{
		public Book() 
		{ 
			Authors = new List<Author>();
			Categories= new List<Category>();
            UsersWish = new List<WishList>();
		}

		public int BookId { get; set; }

		[Required]
		public string Title { get; set; } = null!;

		public string? Description { get; set; }

		public string? Isbn { get; set; }

		public string? PublicationYear { get; set; }

		public bool Available { get; set; } = true;

		public int? Quantity { get; set; }

		public int? Pages { get; set; }
		
		public string? ImageUrl { get; set; }

		public string? Language { get; set; }

		public string? Publisher { get; set; }

		public decimal? SalePrice { get; set; }

		public decimal Price { get; set; }

		/* RELATIONS */

		public ICollection<Author> Authors { get; set; }
		public ICollection<Category> Categories { get; set; }
		public ICollection<WishList> UsersWish { get; set; }
	}
}
