using Libria.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Libria.ViewModels.Account
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = ModelValidationMessages.Required)]
        [DataType(DataType.Password)]
        [DisplayName(ModelDisplayNames.Password)]
		[MinLength(6, ErrorMessage = ModelValidationMessages.Password)]
		public string Password { get; set; } = null!;

        [Required(ErrorMessage = ModelValidationMessages.Required)]
        [DataType(DataType.Password)]
        [DisplayName(ModelDisplayNames.ConfirmPassword)]
        [Compare("Password", ErrorMessage = ModelValidationMessages.PasswordMismatch)]
		[MinLength(6, ErrorMessage = ModelValidationMessages.Password)]
		public string ConfirmPassword { get; set; } = null!;

        public string? Uid { get; set; }

        public string? Token { get; set; }
    }
}
