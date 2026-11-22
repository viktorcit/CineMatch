
using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Data.DTO.ResponsesDto.User;

namespace CineMatch.Api.Services.Interfaces.IUserServices
{
    public interface IUserService
    {
        Task<BaseResponseDto<AdminUserInfoResponseDto>> GetUserInfo(string userId, CancellationToken cancellationToken);
        Task<BaseResponseDto> DeleteUser(string userId);
    }
}
