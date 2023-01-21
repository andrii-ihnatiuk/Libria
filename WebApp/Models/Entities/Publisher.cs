namespace Libria.Models.Entities
{
    public class Publisher
    {
        public int PublisherId { get; set; }

        public string Name { get; set; } = null!;

        public ICollection<Book> Books { get; set; }
    }
}
