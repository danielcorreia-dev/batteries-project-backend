using System.Collections.Generic;

namespace Domain.Entities
{
    public sealed class Company: BaseEntity
    {
        public Company()
        {
            List<UserCompanyPlace> Users = new List<UserCompanyPlace>();
            List<CompanyBenefit> Benefits = new List<CompanyBenefit>();
        }
        public string Title { get; set; }
        public string Address { get; set; }
        public List<UserCompanyPlace> Users { get; set; }
        public List<CompanyBenefit> Benefits { get; set; }
    }
}
