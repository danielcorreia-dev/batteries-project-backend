using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Results
{
    public class UserCompanyScoreModelResult
    {
        [Required] 
        public int CompanyId { get; set; }

        [Required] 
        public int UserId { get; set; }

        [Required] 
        public int Scores { get; set; }
    }
}