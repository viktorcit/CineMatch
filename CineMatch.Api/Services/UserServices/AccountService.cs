using CineMatch.Api.Data;
using CineMatch.Api.Data.DTO;
using CineMatch.Api.Data.DTO.UserDto;
using CineMatch.Api.Enums;
using CineMatch.Api.Helpers;
using Microsoft.EntityFrameworkCore;

namespace CineMatch.Api.Services.UserServices
{
    public class AccountService //: IAccountService
    {
        private readonly ILogger<AccountService> _logger;
        private readonly AppDbContext _db;

        public AccountService(ILogger<AccountService> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }



        public async Task<BaseResponseDto<UserDto>> GetAccountInfo()
        {
            return ErrorFactory.Ok<UserDto>(default, "Account information successfully retrieved.");
        }
    }
}
