using CineMatch.Api.Data.DTO;
using CineMatch.Api.Data.TokensDto;

namespace CineMatch.Api.Services.Interfaces
{
    public interface IJwtTokenService
    {
        string? GenerateAccessToken(AccessTokenRequestDto dto);
    }
}
