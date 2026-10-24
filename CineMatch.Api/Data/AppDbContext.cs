using CineMatch.Api.Entity;
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
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //User
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasIndex(u => u.UserName).IsUnique();
                //entity.Property(u => u.Email).IsRequired();
                entity.Property(u =>u.UserName).IsRequired();
            });

            builder.Entity<ApplicationUser>()
                .HasOne<SessionParticipant>()
                .WithOne()
                .HasForeignKey<SessionParticipant>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasOne<Session>()
                .WithOne()
                .HasForeignKey<Session>(s => s.CreatorUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasMany<RefreshToken>()
                .WithOne()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasMany<Vote>()
                .WithOne()
                .HasForeignKey(v => v.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);

            //Movie
            builder.Entity<Movie>()
                .HasMany<SessionMovie>()
                .WithOne(sm => sm.Movie)
                .HasForeignKey(sm => sm.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Movie>()
                .HasMany<Vote>()
                .WithOne()
                .HasForeignKey(v => v.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Movie>(entity =>
            {
                entity.HasIndex(m => m.TMdbId).IsUnique();
                entity.Property(m => m.TMdbId).IsRequired();
                entity.Property(m => m.Type).IsRequired();
                entity.Property(m => m.Title).IsRequired();
                
            });

            //Refresh Token

            //Session
            builder.Entity<Session>()
                .HasMany<SessionMovie>()
                .WithOne()
                .HasForeignKey(sm => sm.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Session>()
                .HasMany<SessionParticipant>()
                .WithOne()
                .HasForeignKey(sp => sp.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Session>()
                .HasMany<Vote>()
                .WithOne()
                .HasForeignKey(v => v.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Session>(entity =>
            {
                entity.HasIndex(s => s.Code).IsUnique();
                entity.HasIndex(s => s.CreatorUserId).IsUnique();
            });

            //Session Movie
            builder.Entity<SessionMovie>(entity =>
            {
                entity.HasIndex(sm => new
                {
                    sm.SessionId,
                    sm.MovieId
                }).IsUnique();
            });

            //Session Participant
            builder.Entity<SessionParticipant>(entity =>
            {
                entity.HasIndex(sp => sp.UserId).IsUnique();
                entity.HasIndex(sp => new
                {
                    sp.SessionId,
                    sp.UserId
                }).IsUnique();
            });

            //Vote
            builder.Entity<Vote>(entity =>
            {
                entity.HasIndex(v => new
                {
                    v.SessionId,
                    v.ParticipantId,
                    v.MovieId
                }).IsUnique();
            });


        }

    }
}
