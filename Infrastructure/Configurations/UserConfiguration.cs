using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasIndex(x => x.Nick).IsUnique();
            builder.HasIndex(x => x.Email).IsUnique();

            builder.Property(u => u.Nick).HasMaxLength(200);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
            builder.Property(u => u.Password).IsRequired().HasMaxLength(200);

            builder.HasOne(u => u.Company)
                .WithOne(c => c.User)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
