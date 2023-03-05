namespace Libria.Models.Entities
{
    public class Publisher
    {
        public Publisher()
        {
            Books = new List<Book>();
        }

        public int PublisherId { get; set; }
        public string Name { get; set; } = null!;

		/* RELATIONS */
	
        public ICollection<Book> Books { get; set; }
    }
}
