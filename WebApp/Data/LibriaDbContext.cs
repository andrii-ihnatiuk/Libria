using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Libria.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace Libria.Data;

public class LibriaDbContext : IdentityDbContext<User>
{
    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Author> Authors { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Publisher> Publishers { get; set; } = null!;
    public DbSet<WishList> WishList { get; set; } = null!;
    public DbSet<CartUsersBooks> CartUsersBooks { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrdersBooks> OrdersBooks { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;

    public LibriaDbContext(DbContextOptions<LibriaDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Notification entity
        builder.Entity<Notification>()
            .Property(n => n.NotificationType)
            .HasConversion<int>(); // store enum as int in db

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

		builder.Entity<IdentityRole>()
            .HasData(new IdentityRole 
            { 
               Id = "2c5e174e-3b0e-446f-86af-483d56fd7210", Name = "admin", NormalizedName = "ADMIN"
            });

		//a hasher to hash the password before seeding the user to the db
		var hasher = new PasswordHasher<User>();

		//Seeding the User to AspNetUsers table
		builder.Entity<User>().HasData(
			new User
			{
				Id = "8e445865-a24d-4543-a6c6-9443d048cdb9", // primary key
				UserName = "admin@example.com",
                Email = "admin@example.com",
				FirstName = "Admin",
                LastName = "User",
				NormalizedUserName = "admin@example.com".ToUpper(),
				PasswordHash = hasher.HashPassword(null, "admin")
			}
		);

		//Seeding the relation between our user and role to AspNetUserRoles table
		builder.Entity<IdentityUserRole<string>>().HasData(
			new IdentityUserRole<string>
			{
				RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210",
				UserId = "8e445865-a24d-4543-a6c6-9443d048cdb9"
			}
		);

		// Configuring relations

		// Books - Authors
		builder.Entity<Book>()
            .HasMany(b => b.Authors)
            .WithMany(a => a.Books)
            .UsingEntity(b => b
                .HasData(new[]
                {
                    new { BooksBookId = 1, AuthorsAuthorId = 1 },
                    new { BooksBookId = 2, AuthorsAuthorId = 1 }
                }));


        // Books - Categories
        builder.Entity<Category>()
            .HasMany(c => c.Books)
            .WithMany(b => b.Categories);

        // Books - Publisher
        builder.Entity<Book>()
            .HasOne(b => b.Publisher)
            .WithMany(p => p.Books)
            .HasForeignKey(b => b.PublisherId);

		// Users - Books (WishList)
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

        // Users - Books (CartUsersBooks)
        builder.Entity<CartUsersBooks>()
            .HasKey(c => new { c.UserId, c.BookId });

        builder.Entity<CartUsersBooks>()
            .HasOne(c => c.Book)
            .WithMany(b => b.UsersHaveInCart)
            .HasForeignKey(c => c.BookId);

        builder.Entity<CartUsersBooks>()
            .HasOne(c => c.User)
            .WithMany(u => u.BooksInCart)
            .HasForeignKey(c => c.UserId);

        // Orders - Books
        builder.Entity<OrdersBooks>()
            .HasKey(ob => new { ob.OrderId, ob.BookId });

        builder.Entity<OrdersBooks>()
            .HasOne(ob => ob.Book)
            .WithMany(b => b.OrdersContain)
            .HasForeignKey(ob => ob.BookId);

        builder.Entity<OrdersBooks>()
            .HasOne(ob => ob.Order)
            .WithMany(o => o.Books)
            .HasForeignKey(ob => ob.OrderId);

        // Notifications - Books 
        builder.Entity<Book>()
            .HasMany(b => b.Notifications)
            .WithOne(n => n.TargetBook)
            .HasForeignKey(n => n.TargetBookId);

        // Reviews - Books
        builder.Entity<Book>()
            .HasMany(b => b.Reviews)
            .WithOne(r => r.Book)
            .HasForeignKey(r => r.BookId);
	}
}
