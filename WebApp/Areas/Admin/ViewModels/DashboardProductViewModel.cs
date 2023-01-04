using Libria.Data;
using Libria.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Libria.Areas.Admin.ViewModels
{
	public class DashboardProductViewModel
	{
		public string PageTitle { get; set; } = "Редагування товару";

		public string ControllerName { get; set; } = "Products";

		public string ActionName { get; set; } = "Edit";

		public List<SelectListItem> AuthorSelectItems { get; set; } = new();

		public List<SelectListItem> CategorySelectItems { get; set; } = new();

		[Required(ErrorMessage = ModelValidationMessages.Required)]
		public string Title { get; set; } = null!;

		public string? Description { get; set; }

		public string? Isbn { get; set; }

		public string? PublicationYear { get; set; }

		public bool Available { get; set; } = true;

		public int? Quantity { get; set; }

		public int? Pages { get; set; }

		public string? ImageUrl { get; set; }

		public string? Language { get; set; }

		public string? Publisher { get; set; }

		public decimal? SalePrice { get; set; }

		[Required(ErrorMessage = ModelValidationMessages.Required)]
		public decimal Price { get; set; }
	}
}
