using Libria.Models.Entities;

namespace Libria.Areas.Admin.Models
{
	public class CategoryCard
	{
		public Category Category { get; set; } = null!;

		public int ItemsCount { get; set; }
	}
}
