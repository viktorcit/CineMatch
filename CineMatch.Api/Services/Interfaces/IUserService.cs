using CineMatch.Api.Data.DTO;
using CineMatch.Api.Data.DTO.UserDto;

namespace CineMatch.Api.Services.Interfaces
{
    public interface IUserService
    {
        Task<BaseResponseWithDataDto<UserDto>> CreateUser(string? clientId);
    }
}
