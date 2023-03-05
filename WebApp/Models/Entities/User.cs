using Microsoft.AspNetCore.Identity;

namespace Libria.Models.Entities;
public class User : IdentityUser
{
    public User()
    {
        BooksWished = new List<WishList>();
        BooksInCart = new List<CartUsersBooks>();
        Orders = new List<Order>();
    }

    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }

    public string? City { get; set; }
    public string? Address { get; set; }

	/* RELATIONS */

	public ICollection<WishList> BooksWished { get; set; }
    public ICollection<CartUsersBooks> BooksInCart { get; set; }
    public ICollection<Order> Orders { get; set; }
}