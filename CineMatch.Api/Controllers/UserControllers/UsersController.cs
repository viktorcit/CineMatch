using CineMatch.Api.Data.DTO.ResponsesDto.User;
using CineMatch.Api.Enums;
using CineMatch.Api.Helpers;
using CineMatch.Api.Services.Interfaces.IUserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineMatch.Api.Controllers.UserControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }


        [Authorize(Roles = RolesName.Admin)]
        [HttpGet("{userId}")]
        public async Task<ActionResult<AdminUserInfoResponseDto>> GetUserInfoAsync(string userId, CancellationToken cancellationToken)
        {
            var result = await _userService.GetUserInfoAsync(userId, cancellationToken);
            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                ErrorType.ServerError => StatusCode(500, result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [Authorize(Roles = RolesName.Admin)]
        [HttpDelete("{userId}")]
        public async Task<ActionResult> DeleteUserAsync(string userId)
        {
            var result = await _userService.DeleteUserAsync(userId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                ErrorType.Forbidden => StatusCode(403, result.ResponseMessage),
                ErrorType.Conflict => Conflict(result.ResponseMessage),
                ErrorType.ServerError => StatusCode(500, result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }
    }
}
