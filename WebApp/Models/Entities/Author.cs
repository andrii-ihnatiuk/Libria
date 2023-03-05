using System.ComponentModel.DataAnnotations;

namespace Libria.Models.Entities
{
    public class Author
    {
        public Author()
        {
            Books = new List<Book>();
        }

        public int AuthorId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        /* RELATIONS */

        public ICollection<Book> Books { get; set; }
    }
}
