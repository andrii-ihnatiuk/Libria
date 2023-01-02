using Libria.Data;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;

namespace Libria.Areas.Admin.ViewModels
{
	public class DashboardCategoryViewModel
	{
		public int? Id { get; set; }

		[Required(ErrorMessage = ModelValidationMessages.Required)]
		public string Name { get; set; } = null!;

		[MaxLength(2000, ErrorMessage = "Максимальна довжина опису 2000 символів!")]
		public string? Description { get; set; }

		public string PageTitle { get; set; } = "Редагування категорії";

		public string ActionName { get; set; } = "Edit";
	}
}
