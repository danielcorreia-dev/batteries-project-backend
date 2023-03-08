using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class BatteriesProjectDbContext: DbContext, IBatteriesProjectDbContext
    {
        public BatteriesProjectDbContext(DbContextOptions<BatteriesProjectDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }


    }
}
