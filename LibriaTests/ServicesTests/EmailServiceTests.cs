using Libria.Models;
using Libria.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace LibriaTests.ServicesTests
{
	public class EmailServiceTests
	{
		[Fact]
		public async Task SendEmailAsync_WrongConnectionOptions_ExpectErrorResult()
		{
			// Arrange
			var optionsMock = new Mock<IOptions<EmailSettings>>();
			optionsMock.Setup(m => m.Value).Returns(new EmailSettings());
			var service = new EmailService(optionsMock.Object, new Mock<ILogger<EmailService>>().Object);

			// Act
			var result = await service.SendEmailAsync(new List<string>(), "", new List<string>());

			// Assert
			Assert.Equal(EmailStatus.ErrorResult, result);
		}
	}
}
