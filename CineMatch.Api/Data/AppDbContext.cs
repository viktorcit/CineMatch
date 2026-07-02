using CineMatch.Api.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CineMatch.Api.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<SessionMovie> SessionMovies => Set<SessionMovie>();
        public DbSet<Vote> Votes => Set<Vote>();
        public DbSet<SessionParticipant> SessionParticipants => Set<SessionParticipant>();
        public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    }
}
