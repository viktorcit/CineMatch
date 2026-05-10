using CineMatch.Data.DTO;
using CineMatch.Data.DTO.SessionDto;

namespace CineMatch.Services.Interfaces
{
    public interface ISessionService
    {
        Task<BaseResponseWithDataDto<SessionDto>> CreateSessionAsync(string clientId);
    }
}
