using Libria.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Libria.ViewModels
{
	public class ResetPasswordViewModel
	{
		[Required(ErrorMessage = ModelValidationMessages.Required)]
		[EmailAddress(ErrorMessage = ModelValidationMessages.Email)]
		[DisplayName(ModelDisplayNames.Email)]
		public string Email { get; set; } = null!;
	}
}
