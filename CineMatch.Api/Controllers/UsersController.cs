using CineMatch.Api.Data;
using CineMatch.Api.Data.DTO.UserDto;
using CineMatch.Api.Enums;
using CineMatch.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CineMatch.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<UsersController> _logger;
        private readonly IUserService _userService;


        public UsersController(AppDbContext db, ILogger<UsersController> logger, IUserService userService)
        {
            _db = db;
            _logger = logger;
            _userService = userService;
        }




        [HttpPost("guest")]
        public async Task<ActionResult<UserDto>> CreateUser(string clientId)
        {
            var result = await _userService.CreateUser(clientId);

            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }
    }
}
