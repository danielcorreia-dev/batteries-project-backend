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

            builder.Property(u => u.Nick).IsRequired().HasMaxLength(200);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
            builder.Property(u => u.Password).IsRequired().HasMaxLength(200);

            builder.HasMany(u => u.Companies)
                .WithOne(ucp => ucp.User)
                .HasForeignKey(ucp => ucp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
