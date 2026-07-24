using CineMatch.Api.Data.DTO;
using CineMatch.Api.Data.DTO.AuthDto;
using CineMatch.Api.Data.DTO.TokensDto;

namespace CineMatch.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<BaseResponseDto<TokensResponseDto>> RegisterAsync(RegisterRequestDto dto);
        Task<BaseResponseDto<TokensResponseDto>> LoginAsync(LoginRequestDto dto);
        Task<BaseResponseDto<TokensResponseDto>> RefreshUserTokensAsync(RefreshTokenRequestDto dto);
    }
}
