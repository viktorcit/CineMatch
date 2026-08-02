using CineMatch.Api.Data.DTO.RequestsDto.Auth;
using CineMatch.Api.Data.DTO.RequestsDto.Tokens;
using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Data.DTO.ResponsesDto.Tokens;

namespace CineMatch.Api.Services.Interfaces.IUserServices
{
    public interface IAuthService
    {
        Task<BaseResponseDto<TokensResponseDto>> RegisterAsync(RegisterRequestDto dto);
        Task<BaseResponseDto<TokensResponseDto>> LoginAsync(LoginRequestDto dto);
        Task<BaseResponseDto<TokensResponseDto>> RefreshUserTokensAsync(RefreshTokenRequestDto dto);
    }
}
