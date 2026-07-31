using CineMatch.Api.Data.DTO;
using CineMatch.Api.Data.DTO.UserDto;

namespace CineMatch.Api.Services.Interfaces
{
    public interface IAccountService
    {
        Task<BaseResponseDto<UserDto>> GetAccountInfo(string accountId);
        Task<BaseResponseDto<UserDto>> SwitchAccount(string accountId, string secret);
    }
}
