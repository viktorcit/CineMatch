using CineMatch.Api.Configuration;
using CineMatch.Api.Model;
using CineMatch.Api.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace CineMatch.Api.Services.JwtServices
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtTokenService> _logger;

        public JwtTokenService( ILogger<JwtTokenService> logger, IOptions<JwtSettings> options)
        {
            _logger = logger;
            _jwtSettings = options.Value;
        }



        //public string GenerateToken(ApplicationUser user)
        //{
        //    var userId = user.Id;
        //    var userName = user.UserName;
        //    var roles = user.Roles;
        //}
    }
}
