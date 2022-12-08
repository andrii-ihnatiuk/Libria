using Libria.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Libria.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = ModelValidationMessages.Required)]
        [EmailAddress(ErrorMessage = ModelValidationMessages.Email)]
        [DisplayName(ModelDisplayNames.Email)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = ModelValidationMessages.Required)]
        [DataType(DataType.Password)]
        [DisplayName(ModelDisplayNames.Password)]
        public string Password { get; set; } = null!;

        [DisplayName(ModelDisplayNames.RememberMe)]
        public bool RememberMe { get; set; } = false;
    }
}
