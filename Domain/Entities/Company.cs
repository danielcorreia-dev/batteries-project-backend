using System.Collections.Generic;

namespace Domain.Entities
{
    public sealed class Company: BaseEntity
    {
        public Company()
        {
            List<UserCompanyScores> Users = new List<UserCompanyScores>();
            List<CompanyBenefit> Benefits = new List<CompanyBenefit>();
        }
        public string Title { get; set; }
        public string Address { get; set; }
        public List<UserCompanyScores> Users { get; set; }
        public List<CompanyBenefit> Benefits { get; set; }
        
    }
}
