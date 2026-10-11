using CineMatch.Api.Data.DTO.RequestsDto.Account;
using CineMatch.Api.Data.DTO.ResponsesDto.User;
using CineMatch.Api.Enums;
using CineMatch.Api.Extensions;
using CineMatch.Api.Services.Interfaces.IUserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineMatch.Api.Controllers.UserControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }


        [Authorize]
        [HttpGet]
        public async Task<ActionResult<PrivateUserResponseDto>> GetAccountInfoAsync()
        {
            var userId = User.GetUserId();
            var result = await _accountService.GetAccountInfoAsync(userId);
            return result.ErrorType switch
            {
                ErrorType.Unauthorized => Unauthorized(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [HttpGet("{userName}")]
        public async Task<ActionResult<PublicUserResponseDto>> GetAccountByUserNameAsync(string userName)
        {
            var result = await _accountService.GetAccountByUserNameAsync(userName);
            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [Authorize]
        [HttpPut]
        public async Task<ActionResult> UpdateAccountInfoAsync(UpdateAccountRequestDto dto)
        {
            var userId = User.GetUserId();
            var result = await _accountService.UpdateAccountInfoAsync(userId, dto);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.Unauthorized => Unauthorized(result.ResponseMessage),
                ErrorType.Conflict => Conflict(result.ResponseMessage),
                ErrorType.ServerError => StatusCode(500, result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }

        [Authorize]
        [HttpDelete]
        public async Task<ActionResult> DeleteAccountAsync()
        {
            var userId = User.GetUserId();
            var result = await _accountService.DeleteAccountAsync(userId);
            return result.ErrorType switch
            {
                ErrorType.Unauthorized => Unauthorized(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }

        [Authorize]
        [HttpPut("password")]
        public async Task<ActionResult> ChangePasswordAsync(ChangePasswordRequestDto dto)
        {
            var userId = User.GetUserId();
            var result = await _accountService.ChangePasswordAsync(userId, dto);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.Unauthorized => Unauthorized(result.ResponseMessage),
                ErrorType.Conflict => Conflict(result.ResponseMessage),
                ErrorType.ServerError => StatusCode(500, result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }
    }
}
