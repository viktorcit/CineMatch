using CineMatch.Api.Data;
using CineMatch.Api.Entity;
using CineMatch.Api.Services.Interfaces.IJwtServices;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CineMatch.Api.Services.JwtServices
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly ILogger<RefreshTokenService> _logger;
        private readonly AppDbContext _db;


        public RefreshTokenService(AppDbContext db, ILogger<RefreshTokenService> logger)
        {
            _logger = logger;
            _db = db;
        }


        public async Task<string?> RefreshToken(string oldRefreshToken)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "RefreshToken" });
            var storedToken = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == oldRefreshToken);
            if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow || storedToken.RevokedAt != null)
            {
                _logger.LogWarning("Refresh rejected: token is {Reason}", storedToken == null ? "unknown" : storedToken.RevokedAt != null ? "revoked" : "expired");
                return null;
            }

            var newRefreshToken = await GenerateUniqueRefreshToken();
            var refreshToken = CreateRefreshTokenEntity(storedToken.UserId, newRefreshToken);
            storedToken.RevokedAt = DateTime.UtcNow;
            _db.RefreshTokens.Add(refreshToken);
            _db.RefreshTokens.Update(storedToken);
            await _db.SaveChangesAsync();

            _logger.LogDebug("Refresh token rotated for user {UserId}", storedToken.UserId);
            return newRefreshToken;
        }

        public async Task<string> CreateRefreshToken(string userId)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "CreateRefreshToken", ["UserId"] = userId });
            var newRefreshToken = await GenerateUniqueRefreshToken();
            var refreshTokenEntity = CreateRefreshTokenEntity(userId, newRefreshToken);
            _db.RefreshTokens.Add(refreshTokenEntity);
            await _db.SaveChangesAsync();


            return newRefreshToken;
        }


        //private methods
        private static RefreshToken CreateRefreshTokenEntity(string userId, string token)
        {
            var refreshToken = new RefreshToken
            {
                Token = token,
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            };

            return refreshToken;
        }

        private async Task<string> GenerateUniqueRefreshToken()
        {
            var randomNumber = new byte[64];

            while (true)
            {
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomNumber);
                }
                var existingToken = await _db.RefreshTokens.AnyAsync(t => t.Token == Convert.ToBase64String(randomNumber));
                if (!existingToken)
                {
                    var token = Convert.ToBase64String(randomNumber);

                    return token;
                }
            }
        }
    }
}
