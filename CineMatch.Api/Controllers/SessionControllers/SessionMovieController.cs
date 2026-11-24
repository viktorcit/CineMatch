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
        private readonly ISessionMovieService _sessionMovieService;

        public SessionMovieController(ISessionMovieService sessionMovieService)
        {
            _sessionMovieService = sessionMovieService;
        }


        [Authorize]    
        [HttpGet("movies/{sessionCode}")]
        public async Task<ActionResult<List<MovieInfo>>> GetFilmsOfSessionAsync(string sessionCode, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();

            var result = await _sessionMovieService.GetFilmsOfSessionAsync(sessionCode, userId, cancellationToken);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [HttpGet("matched/{sessionCode}")]
        public async Task<ActionResult<List<MovieInfo>>> GetMatchedFilmsOfSessionAsync(string sessionCode, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();

            var result = await _sessionMovieService.GetMatchedInSessionMovieAsync(sessionCode, userId, cancellationToken);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [HttpGet("random-film/{sessionCode}")]
        public async Task<ActionResult<MovieInfo>> GetRandomMatchedFilmAsync(string sessionCode, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();

            var result = await _sessionMovieService.GetRandomMatchedFilmAsync(sessionCode, userId, cancellationToken);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMovieFromSessionAsync(int id)
        {
            var userId = User.GetUserId();
            var result = await _sessionMovieService.DeleteMovieFromSessionAsync(id, userId);
            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }
    }
}
