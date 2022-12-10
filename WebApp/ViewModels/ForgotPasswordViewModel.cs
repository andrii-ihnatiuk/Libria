using Libria.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Libria.ViewModels
{
	public class ForgotPasswordViewModel
	{
		[Required(ErrorMessage = ModelValidationMessages.Required)]
		[EmailAddress(ErrorMessage = ModelValidationMessages.Email)]
		[DisplayName(ModelDisplayNames.Email)]
		public string Email { get; set; } = null!;

		public bool RequestProcessed { get; set; } = false;
	}
}
