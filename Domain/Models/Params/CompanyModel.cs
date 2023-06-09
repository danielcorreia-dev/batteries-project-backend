using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Params
{
    public record CompanyModel
        ( [Required] string Title, [Required] string Address,
            [Required] string PhoneNumber, [Required] string OpeningHours);
}