using Libria.Data;
using System.ComponentModel.DataAnnotations;

namespace Libria.ViewModels.Account
{
	public class SettingsViewModel
	{
		public UpdateField? UpdateField { get; set; }

		[Required(ErrorMessage = ModelValidationMessages.Required)]
		[Display(Name = ModelDisplayNames.FirstName)]
		public string FirstName { get; set; } = null!;

		[Display(Name = ModelDisplayNames.LastName)]
		public string? LastName { get; set; }

		[Required(ErrorMessage = ModelValidationMessages.Required)]
		[EmailAddress(ErrorMessage = ModelValidationMessages.Email)]
		[Display(Name = ModelDisplayNames.Email)]
		public string Email { get; set; } = null!;

		[Required(ErrorMessage = ModelValidationMessages.Required)]
		[DataType(DataType.Password)]
		[Display(Name = ModelDisplayNames.Password)]
		[MinLength(6, ErrorMessage = ModelValidationMessages.Password)]
		public string Password { get; set; } = null!;

		[Required(ErrorMessage = ModelValidationMessages.Required)]
		[DataType(DataType.Password)]
		[Display(Name = ModelDisplayNames.OldPassword)]
		public string OldPassword { get; set; } = null!;

		[Required(ErrorMessage = ModelValidationMessages.Required)]
		[Phone(ErrorMessage = ModelValidationMessages.PhoneNumber)]
		[RegularExpression(@"^\+?3?8?(0\d{9})$", ErrorMessage = ModelValidationMessages.PhoneNumber)]
		[Display(Name = ModelDisplayNames.PhoneNumber)]
		public string PhoneNumber { get; set; } = null!;
	}

	public enum UpdateField
	{
		Name,
		Email,
		PhoneNumber,
		Password
	}
}
