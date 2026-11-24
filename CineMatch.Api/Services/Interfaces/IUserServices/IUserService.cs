
using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Data.DTO.ResponsesDto.User;

namespace CineMatch.Api.Services.Interfaces.IUserServices
{
    public interface IUserService
    {
        Task<BaseResponseDto<AdminUserInfoResponseDto>> GetUserInfoAsync(string userId, CancellationToken cancellationToken);
        Task<BaseResponseDto> DeleteUserAsync(string userId);
    }
}
