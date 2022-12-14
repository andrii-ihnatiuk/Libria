using Microsoft.AspNetCore.Mvc;

namespace Libria.Controllers
{
	public class CartController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
