using CineMatch.Data;
using CineMatch.Data.DTO;
using CineMatch.Data.DTO.MoviesDto;
using CineMatch.Data.DTO.MoviesDTO;
using CineMatch.Enums;
using CineMatch.Model;
using CineMatch.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineMatch.Services
{
    public class MovieService : IMovieService
    {
        private readonly ILogger<MovieService> _logger;
        private readonly AppDbContext _db;
        public MovieService(ILogger<MovieService> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }




        public async Task<BaseResponseWithDataDto<MovieDto>> SaveMovieAsync(MovieDto dto)
        {
            _logger.LogInformation("Сохранение фильма");
            if (dto == null)
            {
                _logger.LogInformation("Нет данных для сохранения");
                return new BaseResponseWithDataDto<MovieDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Movie data cannot be null."
                };
            }
            if(string.IsNullOrEmpty(dto.Title))
            {
                _logger.LogInformation("Название фильма не указано");
                return new BaseResponseWithDataDto<MovieDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Movie title is required."
                };
            }
            if(dto.TMdbId <= 0)
            {
                _logger.LogInformation("Некорректный TMDb ID");
                return new BaseResponseWithDataDto<MovieDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "TMDb ID must be a positive integer."
                };
            }
            if(dto.Year.HasValue && (dto.Year < 1888 || dto.Year > DateTime.Now.Year + 1))
            {
                _logger.LogInformation("Некорректный год выпуска");
                return new BaseResponseWithDataDto<MovieDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Year must be between 1888 and next year."
                };
            }

            var movieExists = await _db.Movies.AnyAsync(m => m.TMdbId == dto.TMdbId && m.Type == dto.Type);
            if (movieExists)
            {
                _logger.LogInformation("Фильм уже существует в базе данных");
                return new BaseResponseWithDataDto<MovieDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Conflict,
                    ResponseMessage = "Movie with the same TMDb ID and type already exists."
                };
            }

            var movie = new Movie
            {
                TMdbId = dto.TMdbId,
                Type = dto.Type,
                Title = dto.Title,
                Year = dto.Year,
                Overview = dto.Overview,
                PosterUrl = dto.PosterUrl,
                Genres = dto.Genres
            };

            _db.Movies.Add(movie);
            await _db.SaveChangesAsync();

            var response = new MovieDto
            {
                Id = movie.Id,
                TMdbId = movie.TMdbId,
                Type = movie.Type,
                Title = movie.Title,
                Year = movie.Year,
                Overview = movie.Overview,
                PosterUrl = movie.PosterUrl,
                Genres = movie.Genres
            };

            return new BaseResponseWithDataDto<MovieDto>
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "Movie saved successfully (placeholder).",
                Data = response
            };
        }
    }
}
