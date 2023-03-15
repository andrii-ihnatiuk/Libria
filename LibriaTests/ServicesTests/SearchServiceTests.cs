using Libria.Data;
using Libria.Services;
using Microsoft.EntityFrameworkCore;

namespace LibriaTests.ServicesTests
{
	public class SearchServiceTests
	{

		[Fact]
		public async Task SearchAsync_NoParametersPassed_ExpectNull()
		{
			// Arrange
			var options = new DbContextOptionsBuilder<LibriaDbContext>().UseInMemoryDatabase("testsDB").Options;
			var context = new LibriaDbContext(options);
			var service = new SearchService(context);

			// Act
			var result = await service.SearchAsync();

			// Assert
			Assert.Null(result);
		}
	}
}
