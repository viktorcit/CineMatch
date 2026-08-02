using CineMatch.Api.Data;
using CineMatch.Api.Data.DTO.RequestsDto.Tokens;
using CineMatch.Api.Entity;
using CineMatch.Api.Services.Interfaces.IJwtServices;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CineMatch.Api.Services.JwtServices
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly AppDbContext _db;


        public RefreshTokenService(AppDbContext db)
        {
            _db = db;
        }


        public async Task<string?> RefreshToken(RefreshTokenRequestDto dto)
        {
            var storedToken = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == dto.OldRefreshToken && t.UserId == dto.UserId);
            if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow || storedToken.RevokedAt != null)
            {
                return null;
            }

            var newRefreshToken = await GenerateUniqueRefreshToken();
            var refreshToken = CreateRefreshTokenEntity(dto.UserId, newRefreshToken);
            storedToken.RevokedAt = DateTime.UtcNow;
            _db.RefreshTokens.Add(refreshToken);
            _db.RefreshTokens.Update(storedToken);
            await _db.SaveChangesAsync();

            return newRefreshToken;
        }

        public async Task<string> CreateRefreshToken(string userId)
        {
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
