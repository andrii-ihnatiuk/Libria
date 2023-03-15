using Libria.Controllers;
using Libria.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace LibriaTests.ControllersTests
{
	public class WishListControllerTests
	{
		[Fact]
		public async Task Index_UserIdPresent_ExpectToCallWishListService()
		{
			// Arrange
			var wishListServiceMock = new Mock<IWishListService>();

			var httpContextMock = new Mock<HttpContext>();

			var claims = new List<Claim>() { new Claim(ClaimTypes.NameIdentifier, "someUserId") };
			var claimsIdentity = new ClaimsIdentity(claims);
			httpContextMock.SetupGet(x => x.User).Returns(new ClaimsPrincipal(claimsIdentity));
			var controllerContext = new ControllerContext() { HttpContext = httpContextMock.Object };
			var controller = new WishListController(wishListServiceMock.Object) { ControllerContext = controllerContext };

			// Act
			var result = await controller.Index();

			// Assert
			wishListServiceMock.Verify(x => x.GetUserWishListBooksAsync(It.Is<string>(s => s == "someUserId")), Times.Once());
			Assert.IsType<ViewResult>(result);
		}

		[Fact]
		public async Task Add_BookIdIsNull_ExpectSuccessFalse()
		{
			// Arrange
			var wishListServiceMock = new Mock<IWishListService>();

			var httpContextMock = new Mock<HttpContext>();

			var claims = new List<Claim>() { new Claim(ClaimTypes.NameIdentifier, "") };
			var claimsIdentity = new ClaimsIdentity(claims);
			httpContextMock.SetupGet(x => x.User).Returns(new ClaimsPrincipal(claimsIdentity));
			var controllerContext = new ControllerContext() { HttpContext = httpContextMock.Object };

			var controller = new WishListController(wishListServiceMock.Object) { ControllerContext = controllerContext };

			// Act
			var result = await controller.Add(bookId: null);

			// Assert
			Assert.IsType<JsonResult>(result);
			var jsonResult = result as JsonResult;
			Assert.Equal(false, jsonResult?.Value?.GetType()?.GetProperty("success")?.GetValue(jsonResult.Value));
		}

		[Fact]
		public async Task Remove_BookIdNotNull_ExpectToCallWishListService()
		{
			// Arrange
			var wishListServiceMock = new Mock<IWishListService>();

			var httpContextMock = new Mock<HttpContext>();

			var claims = new List<Claim>() { new Claim(ClaimTypes.NameIdentifier, "someUserId") };
			var claimsIdentity = new ClaimsIdentity(claims);
			httpContextMock.SetupGet(x => x.User).Returns(new ClaimsPrincipal(claimsIdentity));
			var controllerContext = new ControllerContext()
			{
				HttpContext = httpContextMock.Object
			};

			var controller = new WishListController(wishListServiceMock.Object)
			{
				ControllerContext = controllerContext
			};

			// Act
			_ = await controller.Remove(1);

			// Assert
			wishListServiceMock.Verify(x => x.RemoveFromUserWishListAsync(It.Is<string>(v => v == "someUserId"), It.Is<int>(v => v == 1)), Times.Once());
		}
	}
}
