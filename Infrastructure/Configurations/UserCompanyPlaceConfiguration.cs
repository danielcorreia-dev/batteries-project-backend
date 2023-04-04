using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public sealed class UserCompanyPlaceConfiguration : IEntityTypeConfiguration<UserCompanyPlace>
    {
        public void Configure(EntityTypeBuilder<UserCompanyPlace> builder)
        {
            builder.Property(ucp => ucp.Place).IsRequired();

            builder.HasKey(ucp => new { ucp.UserId, ucp.CompanyId });

            builder.HasOne(ucp => ucp.User)
               .WithMany(u => u.Companies)
               .HasForeignKey(ucp => ucp.UserId)
               .OnDelete(DeleteBehavior.Cascade);


            builder.HasOne(ucp => ucp.Company)
              .WithMany(c => c.Users)
              .HasForeignKey(ucp => ucp.CompanyId)
              .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
