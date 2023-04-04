namespace Domain.Entities
{
    public sealed class CompanyBenefit: BaseEntity
    {
        public Company Company { get; set; }
        public int CompanyId { get; set; }
        public string Benefit { get; set; }
        public string Description { get; set; }
        public int ScoreNeeded { get; set; }
        public bool Disabled { get; set; }
    }
}
