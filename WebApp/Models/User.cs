using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Libria.Models;
public class User : IdentityUser
{
    public User() 
    {
        BooksWished = new List<WishList>();
        CartItems = new List<Book>();
    }

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    /* RELATIONS */

    public ICollection<WishList> BooksWished { get; set; }
    [NotMapped]
    public ICollection<Book> CartItems { get; set; }
}