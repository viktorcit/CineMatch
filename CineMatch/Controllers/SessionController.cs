using CineMatch.Data.DTO.SessionDto;
using CineMatch.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CineMatch.Controllers
{
    [ApiController]
    [Route("session")]
    public class SessionController : ControllerBase
    {
        private readonly ILogger<SessionController> _logger;
        private readonly ISessionService _sessionService;

        public SessionController(ILogger<SessionController> logger, ISessionService sessionService)
        {
            _logger = logger;
            _sessionService = sessionService;
        }



        [HttpPost]
        public async Task<IActionResult> CreateSession(CreateSessionRequestDto request)
        {
            _logger.LogInformation("создание сессии");

            var result = await _sessionService.CreateSessionAsync(request.ClientId);

                return result.ErrorType switch
            {
                Enums.ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }
    }
}
