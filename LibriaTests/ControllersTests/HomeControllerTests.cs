using Libria.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace LibriaTests.ControllersTests
{
	public class HomeControllerTests
	{
		[Fact]
		public void Index_NoInput_ExpectActionResult()
		{
			// Arrange
			var controller = new HomeController();

			// Act
			var result = controller.Index();

			// Assert
			Assert.IsType<ViewResult>(result);
		}
	}
}
