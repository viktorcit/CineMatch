using CineMatch.Api.Configuration;
using CineMatch.Api.Data.TokensDto;
using CineMatch.Api.Services.Interfaces;
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
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public JwtTokenService(IOptions<JwtSettings> options, JwtSecurityTokenHandler tokenHandler)
        {
            _jwtSettings = options.Value;
            _tokenHandler = tokenHandler;
        }



        public string? GenerateAccessToken(AccessTokenRequestDto dto)
        {
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, dto.UserId),
                new Claim(ClaimTypes.Name, dto.UserName)
            };

            foreach (var role in dto.UserRoles)
            {
                if (!string.IsNullOrWhiteSpace(role))
                    claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var secretKey = _jwtSettings.SecretKey;
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                return null;
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var signing = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                claims: claims,
                audience: _jwtSettings.Audience,
                expires: _jwtSettings.ExpirationTime,
                signingCredentials: signing
                );

            var tokenString = _tokenHandler.WriteToken(token);

            return tokenString;
        }
    }
}
