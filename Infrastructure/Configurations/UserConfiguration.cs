using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.Name).IsRequired();
            builder.Property(u => u.Role).IsRequired();
            builder.Property(u => u.Email).IsRequired();
            builder.Property(u => u.Password).IsRequired();
        }
    }
}
