using CineMatch.Api.Data.DTO.RequestsDto.Account;
using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Data.DTO.ResponsesDto.User;
using CineMatch.Api.Entity;
using CineMatch.Api.Helpers;

namespace CineMatch.Api.Services.Interfaces.IUserServices
{
    public interface IAccountService
    {
        Task<BaseResponseDto<PrivateUserResponseDto>> GetAccountInfoAsync(string userId);

        Task<BaseResponseDto<PublicUserResponseDto>> GetAccountByUserNameAsync(string userName);

        Task<BaseResponseDto> UpdateAccountInfoAsync(string userId, UpdateAccountRequestDto dto);

        Task<BaseResponseDto> DeleteAccountAsync(string userId);

        Task<BaseResponseDto> ChangePasswordAsync();
    }
}
