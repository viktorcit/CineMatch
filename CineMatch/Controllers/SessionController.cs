using CineMatch.Data.DTO.MoviesDto;
using CineMatch.Data.DTO.SessionDto;
using CineMatch.Enums;
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



        // GET
        [HttpGet("movies/{clientId}")]
        public async Task<ActionResult<List<MovieDto>>> GetFilmsOfSessionAsync(string clientId)
        {
            _logger.LogInformation("получение фильмов сессии");
            var result = await _sessionService.GetFilmsOfSessionAsync(clientId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [HttpGet("matched/{clientId}")]
        public async Task<ActionResult<List<MovieDto>>> GetMatchedFilmsOfSessionAsync(string clientId)
        {
            _logger.LogInformation("получение совпадающих фильмов сессии");
            var result = await _sessionService.GetMatchedInSessionMovieAsync(clientId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [HttpGet("random-film/{clientId}")]
        public async Task<ActionResult<MovieDto>> GetRandomMatchedFilmAsync(string clientId)
        {
            _logger.LogInformation("получение случайного фильма сессии");
            var result = await _sessionService.GetRandomMatchedFilmAsync(clientId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }


        // POST
        [HttpPost("create")]
        public async Task<ActionResult<MovieDto>> CreateSessionAsync(CreateSessionRequestDto dto)
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
        public async Task<ActionResult<MovieDto>> JoinToSessionAsync(JoinSessionRequestDto dto)
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
        public async Task<ActionResult<MovieDto>> LeaveSessionAsync(string clientId)
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
        public async Task<ActionResult<MovieDto>> EndSessionAsync(string clientId)
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

        [HttpPost("like")]
        public async Task<ActionResult<MovieDto>> LikeFilmsAsync(VoteRequestDto dto)
        {
            _logger.LogInformation("лайк фильма");
            var result = await _sessionService.LikeFilmsAsync(dto.clientId, dto.movieId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }

        [HttpPost("dislike")]
        public async Task<ActionResult<MovieDto>> DislikeFilmsAsync(VoteRequestDto dto)
        {
            _logger.LogInformation("дизлайк фильма");
            var result = await _sessionService.DislikeFilmsAsync(dto.clientId, dto.movieId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }

        [HttpPost("clear-votes/{clientId}")]
        public async Task<ActionResult<MovieDto>> ClearSessionVotesAsync(string clientId)
        {
            _logger.LogInformation("очистить голоса сессии");
            var result = await _sessionService.ClearSessionVotesAsync(clientId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }
    }
}
