using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Libria.Models;

namespace Libria.Data;

public class LibriaDbContext : IdentityDbContext<User>
{
    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Author> Authors { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<WishList> WishList { get; set; } = null!;

    public LibriaDbContext(DbContextOptions<LibriaDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Seed tables

        builder.Entity<Book>()
            .HasData(new[]
            {
                new Book { BookId = 1, Title = "Test book", Description = "Book description", Quantity = 100 },
				new Book { BookId = 2, Title = "book #2", Description = "New description", Quantity = 50 }
			});

        builder.Entity<Author>()
            .HasData(new[]
            {
                new Author { AuthorId = 1, Name = "Author 1" }
            });

        // Configuring relations

        // Books-Authors
        builder
            .Entity<Book>()
            .HasMany(b => b.Authors)
            .WithMany(a => a.Books)
            .UsingEntity(b => b
                .HasData(new[]
                {
                    new { BooksBookId = 1, AuthorsAuthorId = 1 },
                    new { BooksBookId = 2, AuthorsAuthorId = 1 }
                }));


        // Books-Categories
        builder
            .Entity<Category>()
            .HasMany(c => c.Books)
            .WithMany(b => b.Categories);


		//Users - Books(WishList)
        builder.Entity<WishList>()
            .HasKey(wl => new { wl.UserId, wl.BookId });

		builder.Entity<WishList>()
            .HasOne(wl => wl.User)
            .WithMany(u => u.BooksWished)
            .HasForeignKey(wl => wl.UserId);

        builder.Entity<WishList>()
            .HasOne(wl => wl.Book)
            .WithMany(b => b.UsersWish)
            .HasForeignKey(wl => wl.BookId);
    }
}
