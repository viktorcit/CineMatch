using CineMatch.Api.Data.DTO.ResponsesDto.User;
using CineMatch.Api.Enums;
using CineMatch.Api.Services.Interfaces.IUserServices;
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
    }
}
