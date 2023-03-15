using Libria.Controllers;
using Libria.Data;
using Libria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LibriaTests.ControllersTests
{
	public class AuthorControllerTests
	{
		[Fact]
		public async Task Index_NotExistingAuthor_ExpectNotFound()
		{
			// Arrange
			var options = new DbContextOptionsBuilder<LibriaDbContext>().UseInMemoryDatabase("testsDB").Options;
			var context = new LibriaDbContext(options);
			var searchServiceMock = new Mock<ISearchService>();
			var wishListServiceMock = new Mock<IWishListService>();
			var controller = new AuthorController(context, searchServiceMock.Object, wishListServiceMock.Object);

			// Act
			var result = await controller.Index(1);

			// Assert
			Assert.IsType<NotFoundResult>(result);
		}
	}
}
