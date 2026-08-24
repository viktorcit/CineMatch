using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Data.DTO.ResponsesDto.Session;

namespace CineMatch.Api.Services.Interfaces.ISessionServices
{
    public interface ISessionService
    {
        Task<BaseResponseDto<SessionResponseDto>> CreateSessionAsync(string userId);
        Task<BaseResponseDto> JoinToSessionAsync(string code, string userId);
        Task<BaseResponseDto> LeaveSessionAsync(string userId);
        Task<BaseResponseDto> EndSessionAsync(string userId);
    }
}
