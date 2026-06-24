using CineMatch.Api.Data;
using CineMatch.Api.Data.DTO.UserDto;
using CineMatch.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CineMatch.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IAccountService _accountService;


        public AccountsController(ILogger<UsersController> logger, IAccountService accountService)
        {
            _logger = logger;
            _accountService = accountService;
        }





        [HttpGet("{id}")]
        public async Task<ActionResult> GetAccountInfo(string id)
        {
            if (id == null)
            {
                return BadRequest("Account ID cannot be empty.");
            }
            var result = await _accountService.GetAccountInfo(id);
            return result.ErrorType switch
            {
                Enums.ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                Enums.ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [HttpPost]
        public async Task<ActionResult> SwitchAccount(SwitchAccountRequestDto dto)
        {
            if (dto.PublicId == null || dto.Secret == null)
            {
                return BadRequest("Account ID and secret cannot be empty.");
            }
            var result = await _accountService.SwitchAccount(dto.PublicId, dto.Secret);
            return result.ErrorType switch
            {
                Enums.ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                Enums.ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }
    }
}
