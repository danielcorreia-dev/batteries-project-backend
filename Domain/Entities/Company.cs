using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public sealed class Company: BaseEntity
    {
        public Company()
        {
            List<UserCompanyScore> Users = new List<UserCompanyScore>();
            List<CompanyBenefit> Benefits = new List<CompanyBenefit>();
        }
        public string Title { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string OpeningHours { get; set; }
        public List<UserCompanyScore> Users { get; set; }
        public List<CompanyBenefit> Benefits { get; set; }
        
    }
}
