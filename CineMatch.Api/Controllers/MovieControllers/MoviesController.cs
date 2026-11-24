using CineMatch.Api.Data.Contracts;
using CineMatch.Api.Data.DTO.RequestsDto.Movie;
using CineMatch.Api.Enums;
using CineMatch.Api.Extensions;
using CineMatch.Api.Helpers;
using CineMatch.Api.Services.Interfaces.IMovieServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineMatch.Api.Controllers.MovieControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieSearchService _movieSearchService;
        private readonly IMovieService _movieService;

        public MoviesController(IMovieSearchService movieSearchService, IMovieService movieService)
        {
            _movieSearchService = movieSearchService;
            _movieService = movieService;
        }


        [HttpGet]
        public async Task<ActionResult<List<MovieInfo>>> GetAllMoviesAsync(CancellationToken cancellationToken)
        {
            var result = await _movieService.GetAllMoviesAsync(cancellationToken);
            return result;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MovieInfo>> GetMovieByIdAsync([FromRoute] int id)
        {
            if (id <= 0)
            {
                return NotFound("Movie not found");
            }
            var result = await _movieService.GetMovieByIdAsync(id);
            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [HttpPost("search-by-url")]
        public async Task<ActionResult<MovieInfo>> GetMovieByUrlAsync([FromBody] SearchMovieRequest dto, CancellationToken cancellationToken)
        {
            var result = await _movieSearchService.GetMovieByUrlAsync(dto.MainInput, cancellationToken);

            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                ErrorType.ServerError => StatusCode(500, result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [HttpPost("search")]
        public async Task<ActionResult<List<MovieInfo>>> GetMovieBySearchAsync([FromBody] SearchMovieRequest dto, CancellationToken cancellationToken)
        {
            var result = await _movieSearchService.GetMovieBySearchAsync(dto.MainInput, dto.Type, dto.Year, cancellationToken);


            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                ErrorType.ServerError => StatusCode(500, result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [Authorize]
        [HttpPost("save")]
        public async Task<ActionResult> SaveMovieAsync([FromBody] SaveMovieRequestDto dto, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();

            var result = await _movieService.SaveMovieAsync(dto.TmdbId, dto.Type, userId, cancellationToken);

            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.Conflict => Conflict(result.ResponseMessage),
                ErrorType.ServerError => StatusCode(500, result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }


        [Authorize(Roles = RolesName.Admin)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMovieAsync(int id)
        {
            if (id <= 0)
            {
                return NotFound("Movie not found for deletion");
            }
            var result = await _movieService.DeleteMovieAsync(id);
            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                ErrorType.ServerError => StatusCode(500, result.ResponseMessage),
                _ => Ok()
            };
        }
    }
}
