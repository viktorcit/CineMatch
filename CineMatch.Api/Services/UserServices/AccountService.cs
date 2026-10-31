using CineMatch.Api.Data;
using CineMatch.Api.Data.DTO.RequestsDto.Account;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountService(UserManager<ApplicationUser> userManager)
        {
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

        public async Task<BaseResponseDto> UpdateAccountInfoAsync(string userId, UpdateAccountRequestDto dto)
        {
            if (dto.GetType().GetProperties().All(p =>
            {
                var value = p.GetValue(dto);
                if (value is string text)
                {
                    return string.IsNullOrWhiteSpace(text);
                }
                return value == null;
            }))
            {
                return ResponseFactory.Fail(ErrorType.BadRequest, "No fields to update");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ResponseFactory.Fail(ErrorType.Unauthorized);
            }

            if (!string.IsNullOrWhiteSpace(dto.UserName))
            {
                user.UserName = dto.UserName;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var error = result.Errors.FirstOrDefault();
                    var handlerResponse = IdentityErrorHandler.ErrorHandler(error);
                    return ResponseFactory.Fail(handlerResponse.ErrorType, handlerResponse.ResponseMessage);
                }
            }

            return ResponseFactory.Ok();
        }

        public async Task<BaseResponseDto> DeleteAccountAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ResponseFactory.Fail(ErrorType.Unauthorized);
            }
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var error = result.Errors.FirstOrDefault();
                var handlerResponse = IdentityErrorHandler.ErrorHandler(error);
                return ResponseFactory.Fail(handlerResponse.ErrorType, handlerResponse.ResponseMessage);
            }
            return ResponseFactory.Ok();
        }

        public async Task<BaseResponseDto> ChangePasswordAsync(string userId, ChangePasswordRequestDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ResponseFactory.Fail(ErrorType.Unauthorized);
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var error = result.Errors.FirstOrDefault();
                var handlerResponse = IdentityErrorHandler.ErrorHandler(error);
                return ResponseFactory.Fail(handlerResponse.ErrorType, handlerResponse.ResponseMessage);
            }

            return ResponseFactory.Ok("Password successfully changed.");
        }
    }
}
