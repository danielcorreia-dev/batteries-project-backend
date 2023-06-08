using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.HasIndex(x => x.Title).IsUnique();

            builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Address).IsRequired().HasMaxLength(255);
            builder.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(255);
            builder.Property(c => c.OpeningHours).IsRequired().HasMaxLength(255);
            
            builder.HasMany(c => c.Users)
                .WithOne(ucp => ucp.Company)
                .HasForeignKey(ucp => ucp.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
                
        }
    }
}
