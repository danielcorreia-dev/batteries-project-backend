using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public sealed class CompanyBenefitConfiguration : IEntityTypeConfiguration<CompanyBenefit>
    {
        public void Configure(EntityTypeBuilder<CompanyBenefit> builder)
        {
            builder.Property(cb => cb.Benefit).IsRequired();
            builder.Property(cb => cb.Description).IsRequired();
            builder.Property(cb => cb.ScoreNeeded).IsRequired();

            builder.HasOne(cb => cb.Company)
              .WithMany(c => c.Benefits)
              .HasForeignKey(cb => cb.CompanyId)
              .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
