using System.Linq;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.HasIndex(c => c.Title).IsUnique();
            
            builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Address).IsRequired().HasMaxLength(255);
            builder.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(255);
            builder.Property(c => c.OpeningHours).IsRequired().HasMaxLength(255);
            
            builder.HasOne(c => c.User)
                .WithOne(u => u.Company)
                .OnDelete(DeleteBehavior.Cascade);
                
        }
    }
}
