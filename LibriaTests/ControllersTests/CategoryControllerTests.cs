using Libria.Controllers;
using Libria.Data;
using Libria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LibriaTests.ControllersTests
{
	public class CategoryControllerTests
	{
		[Fact]
		public async Task Index_NotExistingCategory_ExpectNotFound()
		{
			// Arrange
			var options = new DbContextOptionsBuilder<LibriaDbContext>().UseInMemoryDatabase("testsDB").Options;
			var context = new LibriaDbContext(options);
			var wishListServiceMock = new Mock<IWishListService>();
			var searchServiceMock = new Mock<ISearchService>();
			var controller = new CategoryController(context, wishListServiceMock.Object, searchServiceMock.Object);

			// Act
			var result = await controller.Index(1);

			// Assert
			Assert.IsType<NotFoundResult>(result);
		}
	}
}
