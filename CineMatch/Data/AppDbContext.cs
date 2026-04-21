using CineMatch.Model;
using Microsoft.EntityFrameworkCore;

namespace CineMatch.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Movie> Movies => Set<Movie>();
    }
}
