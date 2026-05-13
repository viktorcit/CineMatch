using CineMatch.Data.DTO.MoviesDto;
using CineMatch.Data.DTO.UserDto;
using CineMatch.Enums;
using CineMatch.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CineMatch.Controllers
{
    [ApiController]
    [Route("movies")]
    public class MovieController : ControllerBase
    {
        private readonly IMovieSearchService _movieSearchService;
        private readonly IMovieService _movieService;
        private readonly ILogger<MovieController> _logger;

        public MovieController(IMovieSearchService movieSearchService, ILogger<MovieController> logger, IMovieService movieService)
        {
            _movieSearchService = movieSearchService;
            _logger = logger;
            _movieService = movieService;
        }




        [HttpGet]
        public async Task<ActionResult<List<MovieDto>>> GetAllMoviesAsync()
        {
            _logger.LogInformation("попытка получить все фильмы из базы данных");
            var result = await _movieService.GetAllMoviesAsync();

            return result;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MovieDto>> GetMovieByIdAsync([FromRoute] int id, [FromQuery] ContentType type)
        {
            _logger.LogInformation("попытка получить фильм по id");
            if (id <= 0)
            {
                _logger.LogInformation("Некорректный id");
                return NotFound("Movie not found");
            }
            var result = await _movieService.GetMovieByIdAsync(id);
            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [HttpPost]
        public async Task<ActionResult<MovieDto>> GetMovieByUrlAsync([FromBody] InputFromUserDto dto)
        {
            _logger.LogInformation("попытка найти фильм по ссылке");
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Модель невалидна");
                return BadRequest(ModelState);
            }

            var result = await _movieSearchService.GetMovieByUrlAsync(dto.MainInput);

            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                ErrorType.ServerError => StatusCode(500, result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [HttpPost("search")]
        public async Task<ActionResult<List<MovieDto>>> GetMovieBySearchAsync([FromBody] InputFromUserDto dto)
        {
            _logger.LogInformation("попытка найти фильм по названию");
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Модель невалидна");
                return BadRequest(ModelState);
            }
            var result = await _movieSearchService.GetMovieBySearchAsync(dto.MainInput, dto.Type, dto.Year);


            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                ErrorType.ServerError => StatusCode(500, result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [HttpPost("save")]
        public async Task<ActionResult<MovieDto>> SaveMovieAsync([FromBody] MovieDto movieDto)
        {
            _logger.LogInformation("попытка добавить фильм в базу данных");
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Модель невалидна");
                return BadRequest(ModelState);
            }
            var result = await _movieService.SaveMovieAsync(movieDto);

            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.Conflict => Conflict(result.ResponseMessage),
                ErrorType.ServerError => StatusCode(500, result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMovieAsync(int id)
        {
            _logger.LogInformation("попытка удалить фильм из базы данных");
            var movie = await _movieService.GetMovieByIdAsync(id);
            if (id <= 0)
            {
                _logger.LogInformation("Некорректный id");
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
