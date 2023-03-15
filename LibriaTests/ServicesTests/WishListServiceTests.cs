using Libria.Data;
using Libria.Models.Entities;
using Libria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibriaTests.ServicesTests
{
    public class WishListServiceTests
    {
        private readonly LibriaDbContext _context;

        public WishListServiceTests()
        {
            var options = new DbContextOptionsBuilder<LibriaDbContext>().UseInMemoryDatabase("testsDB").Options;
            _context = new LibriaDbContext(options);
        }

        [Fact]
        public async Task GetUserWishListBooksAsync_AnyInput_ExpectBookList()
        {
            // Arrange
            var service = new WishListService(_context);

            // Act
            var result = await service.GetUserWishListBooksAsync("");

            // Assert
            Assert.IsAssignableFrom<IEnumerable<Book>>(result);
        }

        [Fact]
        public async Task AddToUserWishListAsync_NotExistingUser_ExpectSuccessFalse()
        {
            // Arrange
            string id = "1";
            var user = new User { Id = id };

            if (_context.Users.Contains(user))
            {
                _context.Entry(user).State = EntityState.Deleted;
                _context.SaveChanges();
            }
            var service = new WishListService(_context);

            // Act
            var result = await service.AddToUserWishListAsync(id, 0);

            // Assert
            Assert.IsType<JsonResult>(result);
            var val = result?.Value;
            Assert.Equal(false, val?.GetType()?.GetProperty("success")?.GetValue(val));
        }


        [Fact]
        public async Task RemoveFromUserWishListAsync_ExitstingEntry_ExpectSuccessTrue()
        {
            // Arrange
            WishList entry = new() { UserId = "test", BookId = 1 };

            if (_context.WishList.Contains(entry) == false)
            {
                _context.WishList.Add(entry);
                _context.SaveChanges();
            }
            var service = new WishListService(_context);

            // Act
            var result = await service.RemoveFromUserWishListAsync("test", 1);

            // Assert
            Assert.IsType<JsonResult>(result);
            var val = result?.Value;
            Assert.Equal(true, val?.GetType().GetProperty("success")?.GetValue(val));
        }
    }
}
