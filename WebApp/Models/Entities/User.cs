using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

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

    public string? Country { get; set; }
    public string? City { get; set; }

	/* RELATIONS */

	public ICollection<WishList> BooksWished { get; set; }
    public ICollection<CartUsersBooks> BooksInCart { get; set; }
}