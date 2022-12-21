namespace Libria.Models.Entities
{
    public class CartUsersBooks
    {
        public string UserId { get; set; } = null!;
        public int BookId { get; set; }
        public int Quantity { get; set; } = 1;

        public User User { get; set; } = null!;
        public Book Book { get; set; } = null!;
    }
}
