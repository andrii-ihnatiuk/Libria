using Microsoft.AspNetCore.Identity;

namespace Libria.Models.Entities;
public class User : IdentityUser
{
    public User()
    {
        BooksWished = new List<WishList>();
        BooksInCart = new List<CartUsersBooks>();
    }

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    /* RELATIONS */

    public ICollection<WishList> BooksWished { get; set; }
    public ICollection<CartUsersBooks> BooksInCart { get; set; }
}