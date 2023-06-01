using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Params
{
    public record UserCompanyScoreModel
        (
            [Required]
            int CompanyId,
            [Required]
            int UserId,
            [Required]
            int Scores,
            bool Owner
        );
}