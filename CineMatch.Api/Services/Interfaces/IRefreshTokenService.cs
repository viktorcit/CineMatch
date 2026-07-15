using CineMatch.Api.Data.DTO;
using CineMatch.Api.Data.DTO.TokensDto;

namespace CineMatch.Api.Services.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<string?> RefreshToken(RefreshTokenRequestDto dto);
        Task<string> CreateRefreshToken(string userId);
    }
}
