using CineMatch.Api.Data.DTO;
using CineMatch.Api.Data.DTO.UserDto;

namespace CineMatch.Api.Services.Interfaces
{
    public interface IAccountService
    {
        Task<BaseResponseWithDataDto<UserDto>> GetAccountInfo(string accountId);
        Task<BaseResponseWithDataDto<UserDto>> SwitchAccount(string accountId, string secret);
    }
}
