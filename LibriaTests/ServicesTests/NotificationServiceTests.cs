using Libria.Data;
using Libria.Models.Entities;
using Libria.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LibriaTests.ServicesTests
{
    public class NotificationServiceTests
    {
        private readonly LibriaDbContext _context;

        public NotificationServiceTests()
        {
            var options = new DbContextOptionsBuilder<LibriaDbContext>().UseInMemoryDatabase("testsDB").Options;
            _context = new LibriaDbContext(options);
        }


        [Fact]
        public async Task SubscribeForNotificationAsync_WrongBookId_FailedStatus()
        {
            // Arrange
            var service = new NotificationService(_context, new Mock<IEmailService>().Object);

            // Act
            var status = await service.SubscribeForNotificationAsync("email@example.com", -1, NotificationType.Availability);

            // Assert
            Assert.Equal(NotificationRegisterStatus.Failed, status);
        }

        [Fact]
        public void Unsubscribe_WrongData_FailedStatus() { }


        [Fact]
        public async Task NotifyPriceDropAsync_CorrectData_ExpectGoodResult()
        {
            // Arrange
            _context.Notifications.Add(new Notification
            {
                TargetBookId = 1,
                NotificationType = NotificationType.PriceDrop,
                UserEmail = ""
            });
            _context.SaveChanges();

            var mock = new Mock<IEmailService>();
            mock.Setup(x => x.SendEmailAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync(EmailStatus.SuccessResult);
            var service = new NotificationService(_context, mock.Object);

            // Act
            var status = await service.NotifyPriceDropAsync(new Book { BookId = 1 });

            // Assert
            Assert.Equal(1, status);
        }

        [Fact]
        public async Task NotifyAvailableAsync_CorrectData_ExpectGoodResult()
        {
            // Arrange
            _context.Notifications.Add(new Notification
            {
                TargetBookId = 1,
                NotificationType = NotificationType.Availability,
                UserEmail = ""
            });
            _context.SaveChanges();

            var mock = new Mock<IEmailService>();
            mock.Setup(x => x.SendEmailAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync(EmailStatus.SuccessResult);
            var service = new NotificationService(_context, mock.Object);

            // Act
            var status = await service.NotifyAvailableAsync(new Book { BookId = 1 });

            // Assert
            Assert.Equal(1, status);
        }
    }
}