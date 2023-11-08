using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class BatteriesProjectDbContext: DbContext, IBatteriesProjectDbContext
    {
        public BatteriesProjectDbContext(DbContextOptions<BatteriesProjectDbContext> options) : base(options)
        {}


        public DbSet<UserCompanyScore> UserCompanyScores { get; set; }
        public DbSet<CompanyBenefit> CompanyBenefits { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BatteriesProjectDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
