using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Data.DTO.ResponsesDto.User;

namespace CineMatch.Api.Services.Interfaces.IUserServices
{
    public interface IAccountService
    {
        Task<BaseResponseDto<UserResponseDto>> GetAccountInfo();
    }
}
