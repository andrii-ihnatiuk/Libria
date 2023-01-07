using Libria.Models.Entities;

namespace Libria.Areas.Admin.Models
{
	public class AuthorCard
	{
		public Author Author { get; set; } = null!;

		public int ItemsCount { get; set; }
	}
}
