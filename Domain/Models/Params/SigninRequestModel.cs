using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Params
{
    public record SigninRequestModel
    (
        [EmailAddress]
        [Required]
        string Email,
        [Required]
        string Password,
        string Nick,
        bool RememberMe
    );
}
