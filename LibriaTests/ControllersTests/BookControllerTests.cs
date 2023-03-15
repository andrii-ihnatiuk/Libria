using Libria.Controllers;
using Libria.Data;
using Libria.Models.Entities;
using Libria.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Security.Principal;

namespace LibriaTests.ControllersTests
{
	public class BookControllerTests
	{
		private readonly LibriaDbContext _context;

		public BookControllerTests() 
		{
			var options = new DbContextOptionsBuilder<LibriaDbContext>().UseInMemoryDatabase("testsDB").Options;
			_context = new LibriaDbContext(options);
		}

		[Fact]
		public async Task Index_NotExistingBook_ExpectNotFound()
		{
			// Arrange
			if (_context.Books.Any(b => b.BookId == 1))
			{
				_context.Entry(new Book { BookId = 1 }).State = EntityState.Deleted;
				_context.SaveChanges();
			}
			var notifServiceMock = new Mock<INotificationService>();
			var httpClientFactoryMock = new Mock<IHttpClientFactory>();
			var hostEnvMock = new Mock<IHostEnvironment>();
			var controller = new BookController(_context, notifServiceMock.Object, httpClientFactoryMock.Object, hostEnvMock.Object);

			var httpMock = new Mock<HttpContext>();
			httpMock.SetupGet(x => x.User.Identity).Returns(null as IIdentity);

			var controllerContext = new ControllerContext() { HttpContext = httpMock.Object };
			controller.ControllerContext = controllerContext;

			// Act
			var result = await controller.Index(1);

			// Assert
			Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task NotifyMe_WrongEmail_ExpectBadRequest()
		{
			// Arrange
			var notifServiceMock = new Mock<INotificationService>();
			var httpClientFactoryMock = new Mock<IHttpClientFactory>();
			var hostEnvMock = new Mock<IHostEnvironment>();
			var controller = new BookController(_context, notifServiceMock.Object, httpClientFactoryMock.Object, hostEnvMock.Object);

			// Act
			var result = await controller.NotifyMe(1, "wrongemail", NotificationType.PriceDrop);

			// Assert
			Assert.IsType<BadRequestResult>(result);
		}

		[Fact]
		public void UnsubscribeNotification_NotificationServiceFailed_ExpectNotFound()
		{
			// Arrange
			var notifServiceMock = new Mock<INotificationService>();
			notifServiceMock
				.Setup(x => x.Unsubscribe(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
				.Returns(NotificationRegisterStatus.Failed);
			var httpClientFactoryMock = new Mock<IHttpClientFactory>();
			var hostEnvMock = new Mock<IHostEnvironment>();
			var controller = new BookController(_context, notifServiceMock.Object, httpClientFactoryMock.Object, hostEnvMock.Object);

			// Act
			var result = controller.UnsubscribeNotification(1, "test@example.com", 1);

			// Assert
			Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task AddReview_UserNotAuthenticated_ExpectBadRequest()
		{
			// Arrange
			var notifServiceMock = new Mock<INotificationService>();
			var httpClientFactoryMock = new Mock<IHttpClientFactory>();
			var hostEnvMock = new Mock<IHostEnvironment>();
			var controller = new BookController(_context, notifServiceMock.Object, httpClientFactoryMock.Object, hostEnvMock.Object);

			var httpMock = new Mock<HttpContext>();
			httpMock.SetupGet(x => x.User.Identity!.IsAuthenticated).Returns(false);
			var controllerContext = new ControllerContext() { HttpContext = httpMock.Object };
			controllerContext.HttpContext = httpMock.Object;
			controller.ControllerContext = controllerContext;

			// Act
			var result = await controller.AddReview(1, "", 1);

			// Assert
			Assert.IsType<BadRequestResult>(result);
		}

		[Fact]
		public void SimilarBooks_NotExistingBook_ExpectObjectResult() { }

	}
}
