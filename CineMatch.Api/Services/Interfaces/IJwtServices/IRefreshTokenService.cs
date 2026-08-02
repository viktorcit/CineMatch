using CineMatch.Api.Data.DTO;
using CineMatch.Api.Data.DTO.RequestsDto.Tokens;

namespace CineMatch.Api.Services.Interfaces.IJwtServices
{
    public interface IRefreshTokenService
    {
        Task<string?> RefreshToken(RefreshTokenRequestDto dto);
        Task<string> CreateRefreshToken(string userId);
    }
}
