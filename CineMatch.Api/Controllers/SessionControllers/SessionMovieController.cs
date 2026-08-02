using CineMatch.Api.Data.Contracts;
using CineMatch.Api.Enums;
using CineMatch.Api.Services.Interfaces.ISessionServices;
using Microsoft.AspNetCore.Mvc;

namespace CineMatch.Api.Controllers.SessionControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionMovieController : ControllerBase
    {
        private readonly ILogger<SessionMovieController> _logger;
        private readonly ISessionMovieService _sessionMovieService;

        public SessionMovieController(ILogger<SessionMovieController> logger, ISessionMovieService sessionMovieService)
        {
            _logger = logger;
            _sessionMovieService = sessionMovieService;
        }



        [HttpGet("movies/{clientId}")]
        public async Task<ActionResult<List<MovieInfo>>> GetFilmsOfSessionAsync(string clientId)
        {
            _logger.LogInformation("получение фильмов сессии");
            var result = await _sessionMovieService.GetFilmsOfSessionAsync(clientId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [HttpGet("matched/{clientId}")]
        public async Task<ActionResult<List<MovieInfo>>> GetMatchedFilmsOfSessionAsync(string clientId)
        {
            _logger.LogInformation("получение совпадающих фильмов сессии");
            var result = await _sessionMovieService.GetMatchedInSessionMovieAsync(clientId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [HttpGet("random-film/{clientId}")]
        public async Task<ActionResult<MovieInfo>> GetRandomMatchedFilmAsync(string clientId)
        {
            _logger.LogInformation("получение случайного фильма сессии");
            var result = await _sessionMovieService.GetRandomMatchedFilmAsync(clientId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }
    }
}
