using Libria.Models.Entities;

namespace Libria.Areas.Admin.Models
{
	public class PublisherCard
	{
		public Publisher Publisher{ get; set; } = null!;

		public int ItemsCount { get; set; }
	}
}
