using Libria.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Libria.ViewModels
{
	public class ResetPasswordViewModel
	{
		[Required(ErrorMessage = ModelValidationMessages.Required)]
		[DataType(DataType.Password)]
		[DisplayName(ModelDisplayNames.Password)]
		public string Password { get; set; } = null!;

		[Required(ErrorMessage = ModelValidationMessages.Required)]
		[DataType(DataType.Password)]
		[DisplayName(ModelDisplayNames.ConfirmPassword)]
		[Compare("Password", ErrorMessage = ModelValidationMessages.PasswordMismatch)]
		public string ConfirmPassword { get; set; } = null!;

		public string? Uid { get; set; }

		public string? Token { get; set; }
	}
}
