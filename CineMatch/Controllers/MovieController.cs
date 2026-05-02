using CineMatch.Data.DTO.MoviesDto;
using CineMatch.Data.DTO.MoviesDTO;
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
        public async Task<ActionResult<MovieDto>> GetMovieByUrlAsync(InputFromUserDto dto)
        {
            _logger.LogInformation("попытка найти фильм по ссылке");
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Модель невалидна");
                return BadRequest(ModelState);
            }
            var result = await _movieService.GetMovieByUrlAsync(dto.MainInput);


            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                ErrorType.ServerError => StatusCode(500, result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [HttpGet("/search")]
        public async Task<ActionResult<MovieDto>> GetMovieBySearchAsync(InputFromUserDto dto)
        {
            _logger.LogInformation("попытка найти фильм по названию");
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Модель невалидна");
                return BadRequest(ModelState);
            }
            var result = await _movieService.GetMovieBySearchAsync(dto.MainInput, dto.Type, dto.Year);


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
