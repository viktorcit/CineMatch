using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Data.DTO.ResponsesDto.Session;

namespace CineMatch.Api.Services.Interfaces.ISessionServices
{
    public interface ISessionService
    {
        Task<BaseResponseDto<SessionResponseDto>> CreateSessionAsync(string clientId);
        Task<BaseResponseDto> JoinToSessionAsync(string code, string clientId);
        Task<BaseResponseDto> LeaveSessionAsync(string clientId);
        Task<BaseResponseDto> EndSessionAsync(string clientId);

    }
}
