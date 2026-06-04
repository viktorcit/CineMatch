using CineMatch.Api.Model;
using Microsoft.EntityFrameworkCore;

namespace CineMatch.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<SessionMovie> SessionMovies => Set<SessionMovie>();
        public DbSet<Vote> Votes => Set<Vote>();
        public DbSet<SessionParticipant> SessionParticipants => Set<SessionParticipant>();
        public DbSet<User> Users => Set<User>();
    }
}
