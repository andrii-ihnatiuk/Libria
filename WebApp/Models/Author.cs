using System.ComponentModel.DataAnnotations;

namespace Libria.Models
{
	public class Author
	{
		public Author() 
		{
			Books = new List<Book>();
		}

		public int AuthorId { get; set; }

		[Required]
		public string Name { get; set; } = null!;
		
		public string? Description { get; set; }

		/* RELATIONS */

		public ICollection<Book> Books { get; set; }
	}
}
