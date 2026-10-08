using CineMatch.Api.Data;
using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Data.DTO.ResponsesDto.User;
using CineMatch.Api.Entity;
using CineMatch.Api.Enums;
using CineMatch.Api.Helpers;
using CineMatch.Api.Services.Interfaces.IUserServices;
using Microsoft.AspNetCore.Identity;

namespace CineMatch.Api.Services.UserServices
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountService(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }



        public async Task<BaseResponseDto<PrivateUserResponseDto>> GetAccountInfoAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ResponseFactory.Fail<PrivateUserResponseDto>(ErrorType.Unauthorized, "Account not found");
            }
            var response = new PrivateUserResponseDto
            {
                UserId = user.Id,
                UserName = user.UserName
            };
            return ResponseFactory.Ok(response);
        }

        public async Task<BaseResponseDto<PublicUserResponseDto>> GetAccountByUserNameAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return ResponseFactory.Fail<PublicUserResponseDto>(ErrorType.NotFound);
            }

            var response = new PublicUserResponseDto
            {
                UserId = user.Id,
                UserName = user.UserName
            };

            return ResponseFactory.Ok(response);
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
