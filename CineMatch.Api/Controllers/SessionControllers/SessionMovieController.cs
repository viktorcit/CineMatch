using CineMatch.Api.Data.Contracts;
using CineMatch.Api.Enums;
using CineMatch.Api.Extensions;
using CineMatch.Api.Services.Interfaces.ISessionServices;
using Microsoft.AspNetCore.Authorization;
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


        [Authorize]    
        [HttpGet("movies/{sessionCode}")]
        public async Task<ActionResult<List<MovieInfo>>> GetFilmsOfSessionAsync(string sessionCode)
        {
            var userId = User.GetUserId();

            _logger.LogInformation("получение фильмов сессии");
            var result = await _sessionMovieService.GetFilmsOfSessionAsync(sessionCode, userId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [HttpGet("matched/{sessionCode}")]
        public async Task<ActionResult<List<MovieInfo>>> GetMatchedFilmsOfSessionAsync(string sessionCode)
        {
            var userId = User.GetUserId();

            _logger.LogInformation("получение совпадающих фильмов сессии");
            var result = await _sessionMovieService.GetMatchedInSessionMovieAsync(sessionCode, userId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [HttpGet("random-film/{sessionCode}")]
        public async Task<ActionResult<MovieInfo>> GetRandomMatchedFilmAsync(string sessionCode)
        {
            var userId = User.GetUserId();

            _logger.LogInformation("получение случайного фильма сессии");
            var result = await _sessionMovieService.GetRandomMatchedFilmAsync(sessionCode, userId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }
    }
}
