using CineMatch.Api.Data.DTO;
using CineMatch.Api.Data.DTO.AuthDto;
using CineMatch.Api.Data.DTO.TokensDto;

namespace CineMatch.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<BaseResponseWithDataDto<TokensResponseDto>> RegisterAsync(RegisterRequestDto dto);
        Task<BaseResponseWithDataDto<TokensResponseDto>> LoginAsync(LoginRequestDto dto);
        Task<BaseResponseWithDataDto<TokensResponseDto>> RefreshUserTokensAsync(RefreshTokenRequestDto dto);
    }
}
