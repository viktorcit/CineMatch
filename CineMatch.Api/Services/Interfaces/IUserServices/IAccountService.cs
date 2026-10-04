using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Data.DTO.ResponsesDto.User;
using CineMatch.Api.Entity;
using CineMatch.Api.Helpers;

namespace CineMatch.Api.Services.Interfaces.IUserServices
{
    public interface IAccountService
    {
        Task<BaseResponseDto<UserResponseDto>> GetAccountInfoAsync(ApplicationUser user);

        Task<BaseResponseDto> GetAccountByUserNameAsync();

        Task<BaseResponseDto> UpdateAccountInfoAsync();

        Task<BaseResponseDto> DeleteAccountAsync();

        Task<BaseResponseDto> ChangePasswordAsync();
    }
}
