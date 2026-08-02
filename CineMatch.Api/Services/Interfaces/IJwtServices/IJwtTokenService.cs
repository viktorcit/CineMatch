using CineMatch.Api.Data.Contracts;
using CineMatch.Api.Data.DTO;

namespace CineMatch.Api.Services.Interfaces.IJwtServices
{
    public interface IJwtTokenService
    {
        string? GenerateAccessToken(AccessTokenInfo dto);
    }
}
