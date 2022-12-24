using Libria.Data;
using Libria.ViewModels.Cart;
using System.ComponentModel.DataAnnotations;

namespace Libria.ViewModels.Order
{
	public class OrderDetailsViewModel
	{
		public List<CartItemViewModel>? CartItems { get; set; }

		[Required(ErrorMessage = ModelValidationMessages.Required)]
		[Display(Name = ModelDisplayNames.FirstName)]
		public string FirstName { get; set; } = null!;

		[Required(ErrorMessage = ModelValidationMessages.Required)]
		[Display(Name = ModelDisplayNames.LastName)]
		public string LastName { get; set; } = null!;

		[Required(ErrorMessage = ModelValidationMessages.Required)]
		[Phone(ErrorMessage = ModelValidationMessages.PhoneNumber)]
		[RegularExpression(@"^\+?3?8?(0\d{9})$", ErrorMessage = ModelValidationMessages.PhoneNumber)]
		[Display(Name = ModelDisplayNames.PhoneNumber)]
		public string PhoneNumber { get; set; } = null!;

		[Required(ErrorMessage = ModelValidationMessages.Required)]
		[EmailAddress(ErrorMessage = ModelValidationMessages.Email)]
		[Display(Name = ModelDisplayNames.Email)]
		public string Email { get; set; } = null!;
	}
}
