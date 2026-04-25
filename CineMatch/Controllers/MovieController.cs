using CineMatch.Data.DTO.MoviesDto;
using CineMatch.Enums;
using CineMatch.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CineMatch.Controllers
{
    [ApiController]
    [Route("/movie")]
    public class MovieController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly ILogger<MovieController> _logger;

        public MovieController(IMovieService movieService, ILogger<MovieController> logger)
        {
            _movieService = movieService;
            _logger = logger;
        }




        [HttpGet]
        public async Task<ActionResult<MovieDto>> GetMovie(string inputUrl)
        {
            _logger.LogInformation("попытка найти фильм");
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Модель невалидна");
                return BadRequest(ModelState);
            }
            var result = await _movieService.GetMovieAsync(inputUrl);


            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                ErrorType.ServerError => StatusCode(500, result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }
    }
}
