using CineMatch.Api.Data;
using CineMatch.Api.Data.DTO;
using CineMatch.Api.Data.DTO.AuthDto;
using CineMatch.Api.Enums;
using CineMatch.Api.Model;
using CineMatch.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CineMatch.Api.Services.JwtServices
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;


        public RefreshTokenService(AppDbContext db, UserManager<ApplicationUser> userManager, IJwtTokenService jwtTokenService)
        {
            _db = db;
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
        }





        public async Task<BaseResponseWithDataDto<TokensResponseDto>> RefreshTokens(string? oldRefreshToken)
        {
            var storedToken = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == oldRefreshToken);

            if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
            {
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ResponseMessage = "Invalid or expired refresh token.",
                    ErrorType = ErrorType.Unauthorized,
                    Data = null
                };
            }

            var user = await _userManager.FindByIdAsync(storedToken.UserId);
            if (user == null)
            {
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ResponseMessage = "User not found.",
                    ErrorType = ErrorType.Unauthorized,
                    Data = null
                };
            }

            var newAccessToken = await _jwtTokenService.GenerateTokenAsync(user);
            var newRefreshToken = GenerateUniqueRefreshToken();
            var refreshToken = CreateRefreshTokenEntity(user, newRefreshToken);
            if (!newAccessToken.IsSuccess || newAccessToken.Data == null)
            {
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ResponseMessage = "Failed to generate new access token.",
                    ErrorType = ErrorType.ServerError,
                    Data = null
                };
            }
            storedToken.IsRevoked = true;
            _db.RefreshTokens.Add(refreshToken);
            _db.RefreshTokens.Update(storedToken);
            await _db.SaveChangesAsync();

            var tokensResponse = new TokensResponseDto
            {
                AccessToken = newAccessToken.Data,
                RefreshToken = newRefreshToken
            };

            return new BaseResponseWithDataDto<TokensResponseDto>
            {
                IsSuccess = true,
                ResponseMessage = "Tokens created successfully.",
                ErrorType = ErrorType.None,
                Data = tokensResponse
            };
        }

        public async Task<BaseResponseWithDataDto<string>> CreateRefreshToken(ApplicationUser user)
        {
            var newRefreshToken = GenerateUniqueRefreshToken();
            var refreshToken = CreateRefreshTokenEntity(user, newRefreshToken);
            _db.RefreshTokens.Add(refreshToken);
            await _db.SaveChangesAsync();


            return new BaseResponseWithDataDto<string>
            {
                IsSuccess = true,
                ResponseMessage = "Initial refresh token generated successfully.",
                ErrorType = ErrorType.None,
                Data = newRefreshToken
            };
        }


        //private methods
        private static RefreshToken CreateRefreshTokenEntity(ApplicationUser user, string token)
        {
            var refreshToken = new RefreshToken
            {
                Token = token,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            return refreshToken;
        }

        private string GenerateUniqueRefreshToken()
        {
            var randomNumber = new byte[64];

            while (true)
            {
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomNumber);
                }
                if (!_db.RefreshTokens.Any(t => t.Token == Convert.ToBase64String(randomNumber)))
                {
                    var token = Convert.ToBase64String(randomNumber);

                    return token;
                }
            }
        }
    }
}
