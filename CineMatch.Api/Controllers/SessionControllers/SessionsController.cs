using CineMatch.Api.Data.DTO.RequestsDto.Session;
using CineMatch.Api.Data.DTO.ResponsesDto.Session;
using CineMatch.Api.Enums;
using CineMatch.Api.Extensions;
using CineMatch.Api.Services.Interfaces.ISessionServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineMatch.Api.Controllers.SessionControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionsController : ControllerBase
    {
        private readonly ILogger<SessionsController> _logger;
        private readonly ISessionService _sessionService;

        public SessionsController(ILogger<SessionsController> logger, ISessionService sessionService)
        {
            _logger = logger;
            _sessionService = sessionService;
        }


        // POST
        [Authorize]
        [HttpPost("create")]
        public async Task<ActionResult<SessionResponseDto>> CreateSessionAsync()
        {
            var userId = User.GetUserId();
            _logger.LogInformation("создание сессии");
            var result = await _sessionService.CreateSessionAsync(userId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [Authorize]
        [HttpPost("join")]
        public async Task<ActionResult> JoinToSessionAsync(JoinSessionRequestDto dto)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("присоединение к сессии");
            var result = await _sessionService.JoinToSessionAsync(dto.Code, userId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }

        [Authorize]
        [HttpPost("leave")]
        public async Task<ActionResult> LeaveSessionAsync()
        {
            var userId = User.GetUserId();
            _logger.LogInformation("покинуть сессию");
            var result = await _sessionService.LeaveSessionAsync(userId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }

        [Authorize]
        [HttpPost("end")]
        public async Task<ActionResult> EndSessionAsync()
        {
            var userId = User.GetUserId();
            _logger.LogInformation("завершить сессию");
            var result = await _sessionService.EndSessionAsync(userId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }
    }
}
