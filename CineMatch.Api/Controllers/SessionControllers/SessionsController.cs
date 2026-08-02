using CineMatch.Api.Data.DTO.RequestsDto.Session;
using CineMatch.Api.Data.DTO.ResponsesDto.Session;
using CineMatch.Api.Enums;
using CineMatch.Api.Services.Interfaces.ISessionServices;
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
        [HttpPost("create")]
        public async Task<ActionResult<SessionResponseDto>> CreateSessionAsync(CreateSessionRequestDto dto)
        {
            _logger.LogInformation("создание сессии");
            var result = await _sessionService.CreateSessionAsync(dto.ClientId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [HttpPost("join")]
        public async Task<ActionResult> JoinToSessionAsync(JoinSessionRequestDto dto)
        {
            _logger.LogInformation("присоединение к сессии");
            var result = await _sessionService.JoinToSessionAsync(dto.Code, dto.ClientId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }

        [HttpPost("leave/{clientId}")]
        public async Task<ActionResult> LeaveSessionAsync(string clientId)
        {
            _logger.LogInformation("покинуть сессию");
            var result = await _sessionService.LeaveSessionAsync(clientId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }

        [HttpPost("end/{clientId}")]
        public async Task<ActionResult> EndSessionAsync(string clientId)
        {
            _logger.LogInformation("завершить сессию");
            var result = await _sessionService.EndSessionAsync(clientId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }
    }
}
