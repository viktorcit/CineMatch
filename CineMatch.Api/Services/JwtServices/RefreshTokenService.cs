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
        private readonly ILogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;


        public RefreshTokenService(AppDbContext db, ILogger logger, UserManager<ApplicationUser> userManager, IJwtTokenService jwtTokenService)
        {
            _db = db;
            _logger = logger;
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
                    Data = default
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
                    Data = default
                };
            }

            var newAccessToken = await _jwtTokenService.GenerateTokenAsync(user);
            var newRefreshToken = GenerateRefreshToken();
            var refreshToken = CreateRefreshToken(user, newRefreshToken);
            if (newAccessToken.Data == null)
            {
                return new BaseResponseWithDataDto<TokensResponseDto>
                {
                    IsSuccess = false,
                    ResponseMessage = "Failed to generate new refresh token.",
                    ErrorType = ErrorType.ServerError,
                    Data = default
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
                ResponseMessage = "Token refreshed successfully.",
                ErrorType = ErrorType.None,
                Data = tokensResponse
            };
        }

        public async Task<BaseResponseWithDataDto<string>> InitialRefreshToken()
        {
            return new BaseResponseWithDataDto<string>
            {
                IsSuccess = true,
                ResponseMessage = "Initial refresh token generated successfully.",
                ErrorType = ErrorType.None,
                Data = string.Empty // Temporary solution to return an empty string until the method is configured
            };
        }


        //private methods
        private static RefreshToken CreateRefreshToken(ApplicationUser user, string token)
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

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }

            var token = Convert.ToBase64String(randomNumber);

            return token;
        }
    }
}
