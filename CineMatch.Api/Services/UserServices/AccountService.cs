using CineMatch.Api.Data;
using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Data.DTO.ResponsesDto.User;
using CineMatch.Api.Helpers;
using CineMatch.Api.Services.Interfaces.IUserServices;

namespace CineMatch.Api.Services.UserServices
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _db;

        public AccountService(AppDbContext db)
        {
            _db = db;
        }



        public async Task<BaseResponseDto<UserResponseDto>> GetAccountInfo()
        {
            return ErrorFactory.Ok<UserResponseDto>(default, "Account information successfully retrieved.");
        }
    }
}
