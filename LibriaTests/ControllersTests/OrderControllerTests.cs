using Libria.Controllers;
using Libria.Data;
using Libria.Models.Entities;
using Libria.Services;
using Libria.ViewModels.Cart;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LibriaTests.ControllersTests
{
	public class OrderControllerTests
	{
		[Fact]
		public async Task Index_HasNotAvailableBook_ExpectRedirect()
		{
			// Arrange
			var dbOptions = new DbContextOptionsBuilder<LibriaDbContext>().UseInMemoryDatabase("testsDB1").Options;
			var dbContext = new LibriaDbContext(dbOptions);
			var cartServiceMock = new Mock<ICartService>();
			cartServiceMock
				.Setup(x => x.GetUserCartItemsAsync(It.IsAny<HttpContext>(), It.IsAny<bool>()))
				.ReturnsAsync(new List<CartItemViewModel>() { new CartItemViewModel { Book = new Book { Available = false } } });
			var controller = new OrderController(dbContext, cartServiceMock.Object);

			var tempDataProvider = new Mock<ITempDataProvider>();
			var httpContextMock = new Mock<HttpContext>();
			controller.TempData = new TempDataDictionary(httpContextMock.Object, tempDataProvider.Object);

			// Act
			var result = await controller.Index();

			// Assert
			Assert.IsType<RedirectToActionResult>(result);
		}

		[Fact]
		public void Place_CorrectInput_ExpectToAddOrder()
		{
			// Arrange
			// Act
			// Assert
		}
	}
}
