using System.ComponentModel.DataAnnotations;

namespace Libria.Models.Entities
{
    public class Book
    {
        public Book()
        {
            Authors = new List<Author>();
            Categories = new List<Category>();
            UsersWish = new List<WishList>();
            UsersHaveInCart = new List<CartUsersBooks>();
            OrdersContain = new List<OrdersBooks>();
            Notifications = new List<Notification>();
            Reviews = new List<Review>();
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

        public int? PublisherId { get; set; }

        public decimal SalePrice { get; set; }
       
        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; }

        /* RELATIONS */

        public Publisher? Publisher { get; set; }
        public ICollection<Author> Authors { get; set; }
        public ICollection<Category> Categories { get; set; }
        public ICollection<WishList> UsersWish { get; set; }
        public ICollection<CartUsersBooks> UsersHaveInCart { get; set; }
        public ICollection<OrdersBooks> OrdersContain { get; set; } 
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}
