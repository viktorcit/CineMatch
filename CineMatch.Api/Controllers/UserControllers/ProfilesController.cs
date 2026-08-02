using CineMatch.Api.Services.Interfaces.IUserServices;
using Microsoft.AspNetCore.Mvc;

namespace CineMatch.Api.Controllers.UserControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfilesController : ControllerBase
    {
        private readonly IAccountService _accountService;


        public ProfilesController(IAccountService accountService)
        {
            _accountService = accountService;
        }


        [HttpGet("{id}")]
        public async Task<ActionResult> GetAccountInfo(string id)
        {
            if (id == null)
            {
                return BadRequest("Account ID cannot be empty.");
            }
            var result = await _accountService.GetAccountInfo();
            return result.ErrorType switch
            {
                Enums.ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                Enums.ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }
    }
}
