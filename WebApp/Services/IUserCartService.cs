using Libria.Models;
using Libria.ViewModels.Cart;

namespace Libria.Services
{
	public interface IUserCartService
	{
		public Task<List<CartItemViewModel>> GetUserCartItemsAsync(HttpContext _http);

		public Task<CartActionResult> AddToUserCartAsync(HttpContext _http, int? bookId);
	
		public Task<CartActionResult> RemoveFromUserCartAsync(HttpContext _http, int? bookId, bool fullRemove);
	
		public CartActionResult ClearUserCart(HttpContext _http);
	}
}
