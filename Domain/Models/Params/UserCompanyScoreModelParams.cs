using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Params
{
    public record UserCompanyScoreModelParams
        (
            [Required]
            int CompanyId,
            [Required]
            int UserId,
            [Required]
            int Scores
        );
}