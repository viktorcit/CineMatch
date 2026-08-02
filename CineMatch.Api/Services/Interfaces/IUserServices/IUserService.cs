using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Data.DTO.ResponsesDto.User;

namespace CineMatch.Api.Services.Interfaces.IUserServices
{
    public interface IUserService
    {
        Task<BaseResponseDto<UserResponseDto>> CreateUser(string? clientId);
    }
}
