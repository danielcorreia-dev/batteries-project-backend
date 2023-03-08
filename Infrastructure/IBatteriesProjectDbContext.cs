using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public interface IBatteriesProjectDbContext
    {
        DbSet<T> Set<T>() where T : class;
    }
}
