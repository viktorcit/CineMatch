using CineMatch.Api.Configuration;
using CineMatch.Api.Data.DTO;
using CineMatch.Api.Enums;
using CineMatch.Api.Model;
using CineMatch.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CineMatch.Api.Services.JwtServices
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtTokenService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public JwtTokenService(ILogger<JwtTokenService> logger, IOptions<JwtSettings> options, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _jwtSettings = options.Value;
            _userManager = userManager;
        }



        public async Task<BaseResponseWithDataDto<string>> GenerateTokenAsync(ApplicationUser user)
        {
            if (user == null)
            {
                return new BaseResponseWithDataDto<string>
                {
                    IsSuccess = false,
                    ResponseMessage = "User not found.",
                    ErrorType = ErrorType.NotFound,
                    Data = null
                };
            }

            var roles = await _userManager.GetRolesAsync(user);

            var tokenData = new List<Claim>();

            tokenData.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            tokenData.Add(new Claim(ClaimTypes.Name, user.UserName ?? string.Empty));

            foreach (var role in roles)
            {
                tokenData.Add(new Claim(ClaimTypes.Role, role));
            }

            var secretKey = _jwtSettings.SecretKey;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var signing = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                claims: tokenData,
                audience: _jwtSettings.Audience,
                expires: _jwtSettings.ExpirationMinutes,
                signingCredentials: signing
                );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new BaseResponseWithDataDto<string>
            {
                IsSuccess = true,
                ResponseMessage = "Token generated successfully.",
                ErrorType = ErrorType.None,
                Data = tokenString
            };
        }
    }
}
