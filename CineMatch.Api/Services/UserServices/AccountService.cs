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
        private readonly ILogger<AccountService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountService(UserManager<ApplicationUser> userManager, ILogger<AccountService> logger)
        {
            _logger = logger;
            _userManager = userManager;
        }



        public async Task<BaseResponseDto<PrivateUserResponseDto>> GetAccountInfoAsync(string userId)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "GetAccountInfoAsync", ["UserId"] = userId });
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
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "GetAccountByUserNameAsync" });
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
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "UpdateAccountInfoAsync", ["UserId"] = userId });
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
                    _logger.Log(handlerResponse.ErrorType == ErrorType.ServerError ? LogLevel.Error : LogLevel.Warning, "Account update failed for user {UserId}. Identity codes: {ErrorCodes}", userId, string.Join(", ", result.Errors.Select(e => e.Code)));
                    return ResponseFactory.Fail(handlerResponse.ErrorType, handlerResponse.ResponseMessage);
                }
            }

            _logger.LogInformation("Account update completed for user {UserId}", userId);
            return ResponseFactory.Ok();
        }

        public async Task<BaseResponseDto> DeleteAccountAsync(string userId)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "DeleteAccountAsync", ["UserId"] = userId });
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
                _logger.Log(handlerResponse.ErrorType == ErrorType.ServerError ? LogLevel.Error : LogLevel.Warning, "Account deletion failed for user {UserId}. Identity codes: {ErrorCodes}", userId, string.Join(", ", result.Errors.Select(e => e.Code)));
                return ResponseFactory.Fail(handlerResponse.ErrorType, handlerResponse.ResponseMessage);
            }
            _logger.LogInformation("Account deletion completed for user {UserId}", userId);
            return ResponseFactory.Ok();
        }

        public async Task<BaseResponseDto> ChangePasswordAsync(string userId, ChangePasswordRequestDto dto)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "ChangePasswordAsync", ["UserId"] = userId });
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
                _logger.Log(handlerResponse.ErrorType == ErrorType.ServerError ? LogLevel.Error : LogLevel.Warning, "Password change failed for user {UserId}. Identity codes: {ErrorCodes}", userId, string.Join(", ", result.Errors.Select(e => e.Code)));
                return ResponseFactory.Fail(handlerResponse.ErrorType, handlerResponse.ResponseMessage);
            }

            _logger.LogInformation("Password change completed for user {UserId}", userId);
            return ResponseFactory.Ok("Password successfully changed.");
        }
    }
}
