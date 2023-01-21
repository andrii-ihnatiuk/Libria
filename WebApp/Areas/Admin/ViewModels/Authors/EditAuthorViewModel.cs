using Libria.Data;
using System.ComponentModel.DataAnnotations;

namespace Libria.Areas.Admin.ViewModels.Authors
{
    public class EditAuthorViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = ModelValidationMessages.Required)]
        public string Name { get; set; } = null!;

        [MaxLength(2000, ErrorMessage = "Максимальна довжина опису 2000 символів!")]
        public string? Description { get; set; }

        public string PageTitle { get; set; } = "Редагування автора";

        public string ActionName { get; set; } = "Edit";
    }
}
