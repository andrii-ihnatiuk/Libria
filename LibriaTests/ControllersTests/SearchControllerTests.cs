using Libria.Controllers;
using Libria.Models;
using Libria.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LibriaTests.ControllersTests
{
	public class SearchControllerTests
	{
		[Fact]
		public async Task Index_SearchServiceReturnNull_ExpectNotFound()
		{
			// Arrange
			var wishListServiceMock = new Mock<IWishListService>();
			var searchServiceMock = new Mock<ISearchService>();
			searchServiceMock
				.Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(null as SearchResult);
			var controller = new SearchController(wishListServiceMock.Object, searchServiceMock.Object);

			// Act
			var result = await controller.Index("");

			// Assert
			Assert.IsType<NotFoundResult>(result);
		}
	}
}
