using CineMatch.Api.Data.DTO.AuthDto;
using CineMatch.Api.Data.DTO.TokensDto;
using CineMatch.Api.Enums;
using CineMatch.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CineMatch.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        [HttpPost("register")]
        public async Task<ActionResult<TokensResponseDto>> RegisterAsync(RegisterRequestDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return result.ErrorType switch
            {
                ErrorType.Conflict => Conflict(result.ResponseMessage),
                ErrorType.ServerError => StatusCode(500, result.ResponseMessage),
                _ => Ok(result.Data),
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokensResponseDto>> LoginAsync(LoginRequestDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return result.ErrorType switch
            {
                ErrorType.Unauthorized => Unauthorized(result.ResponseMessage),
                ErrorType.ServerError => StatusCode(500, result.ResponseMessage),
                _ => Ok(result.Data),
            };
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<TokensResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            var result = await _authService.RefreshUserTokensAsync(dto);
            return result.ErrorType switch
            {
                ErrorType.Unauthorized => Unauthorized(result.ResponseMessage),
                ErrorType.ServerError => StatusCode(500, result.ResponseMessage),
                _ => Ok(result.Data),
            };
        }
    }
}
