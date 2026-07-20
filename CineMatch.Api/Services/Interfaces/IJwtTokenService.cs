using CineMatch.Api.Data.DTO;
using CineMatch.Api.Data.DTO.TokensDto;

namespace CineMatch.Api.Services.Interfaces
{
    public interface IJwtTokenService
    {
        string? GenerateAccessToken(AccessTokenDto dto);
    }
}
