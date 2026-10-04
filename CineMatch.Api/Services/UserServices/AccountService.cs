using CineMatch.Api.Data;
using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Data.DTO.ResponsesDto.User;
using CineMatch.Api.Entity;
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



        public async Task<BaseResponseDto<UserResponseDto>> GetAccountInfoAsync(ApplicationUser user)
        {
            var response = new UserResponseDto
            {
                UserId = user.Id,
                UserName = user.UserName
            };
            return ResponseFactory.Ok(response);
        }

        public async Task<BaseResponseDto> GetAccountByUserNameAsync()
        {
            return ResponseFactory.Ok("");
        }

        public async Task<BaseResponseDto> UpdateAccountInfoAsync()
        {
            return ResponseFactory.Ok("");
        }

        public async Task<BaseResponseDto> DeleteAccountAsync()
        {
            return ResponseFactory.Ok("");
        }

        public async Task<BaseResponseDto> ChangePasswordAsync()
        {
            return ResponseFactory.Ok("");
        }
    }
}
