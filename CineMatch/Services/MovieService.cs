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


        public async Task<BaseResponseWithDataDto<SaveMovieDto>> SaveMovieAsync(MovieDto dto, string clientId)
        {
            _logger.LogInformation("Сохранение фильма");
            if (dto == null)
            {
                _logger.LogInformation("Нет данных для сохранения");
                return new BaseResponseWithDataDto<SaveMovieDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Movie data cannot be null."
                };
            }
            if (string.IsNullOrEmpty(dto.Title))
            {
                _logger.LogInformation("Название фильма не указано");
                return new BaseResponseWithDataDto<SaveMovieDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Movie title is required."
                };
            }
            if (dto.TMdbId <= 0)
            {
                _logger.LogInformation("Некорректный TMDb ID");
                return new BaseResponseWithDataDto<SaveMovieDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "TMDb ID must be a positive integer."
                };
            }
            if (dto.Year.HasValue && (dto.Year < 1888 || dto.Year > DateTime.Now.Year + 1))
            {
                _logger.LogInformation("Некорректный год выпуска");
                return new BaseResponseWithDataDto<SaveMovieDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Year must be between 1888 and next year."
                };
            }
            if (string.IsNullOrEmpty(clientId))
            {
                return new BaseResponseWithDataDto<SaveMovieDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Client ID is required."
                };
            }

            var clientSessionExist = await _db.Sessions
                .FirstOrDefaultAsync(cs => cs.Participants.Any(p => p.ClientId == clientId) && cs.IsActive == true);
            if (clientSessionExist == null)
            {
                _logger.LogInformation("Сессия клиента не найдена для Client ID {ClientId} либо был завершена", clientId);
                return new BaseResponseWithDataDto<SaveMovieDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "You are not in any session and cannot save movies or session closed."
                };
            }

            var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == clientSessionExist.Id);
            if (session == null)
            {
                return new BaseResponseWithDataDto<SaveMovieDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Session not found for the client."
                };
            }
            if (!session.IsActive)
            {
                return new BaseResponseWithDataDto<SaveMovieDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Session is closed. You cannot save movies to a closed session."
                };
            }

            var movieExists = await _db.Movies.FirstOrDefaultAsync(m => m.TMdbId == dto.TMdbId && m.Type == dto.Type);
            if (movieExists != null)
            {
                var movieExistSession = await _db.SessionMovies.FirstOrDefaultAsync(sm => sm.MovieId == movieExists.Id && sm.SessionId == session.Id);
                if (movieExistSession != null)
                {
                    _logger.LogInformation("Фильм уже добавлен в сессию и есть в базе данных");
                    return new BaseResponseWithDataDto<SaveMovieDto>
                    {
                        IsSuccess = false,
                        ErrorType = ErrorType.Conflict,
                        ResponseMessage = "Movie with the same TMDb ID and type already exists in the session and database."
                    };
                }
                else if(movieExistSession == null)
                {
                    _logger.LogInformation("Фильм уже существует в базе данных, но не добавлен в сессию. Добавляем фильм в сессию.");
                    var sessionMovie = new SessionMovie
                    {
                        SessionId = session.Id,
                        MovieId = movieExists.Id,
                        Session = session,
                        Movie = movieExists
                    };
                    _db.SessionMovies.Add(sessionMovie);
                    await _db.SaveChangesAsync();
                    var responseOne = new SaveMovieDto
                    {
                        MovieId = movieExists.Id,
                        SessionId = session.Id,
                        TMdbId = movieExists.TMdbId,
                        Type = movieExists.Type,
                        Title = movieExists.Title,
                        Year = movieExists.Year,
                        Overview = movieExists.Overview,
                        PosterUrl = movieExists.PosterUrl,
                        Genres = movieExists.Genres
                    };
                    return new BaseResponseWithDataDto<SaveMovieDto>
                    {
                        IsSuccess = true,
                        ErrorType = ErrorType.None,
                        ResponseMessage = "Movie already exists in the database but has been added to the session.",
                        Data = responseOne
                    };
                }


                _logger.LogInformation("Фильм уже существует в базе данных");
                return new BaseResponseWithDataDto<SaveMovieDto>
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

            var sessionMovieTwo = new SessionMovie
            {
                SessionId = session.Id,
                MovieId = movie.Id,
                Session = session,
                Movie = movie
            };

            _db.Movies.Add(movie);
            _db.SessionMovies.Add(sessionMovieTwo);
            await _db.SaveChangesAsync();

            var responseTwo = new SaveMovieDto
            {
                MovieId = movie.Id,
                TMdbId = movie.TMdbId,
                Type = movie.Type,
                Title = movie.Title,
                Year = movie.Year,
                Overview = movie.Overview,
                PosterUrl = movie.PosterUrl,
                Genres = movie.Genres,
                SessionId = session.Id,
            };

            return new BaseResponseWithDataDto<SaveMovieDto>
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "Movie saved successfully.",
                Data = responseTwo
            };
        }

        public async Task<List<MovieDto>> GetAllMoviesAsync()
        {
            var movies = await _db.Movies
                .Select(m => new MovieDto
                {
                    Id = m.Id,
                    TMdbId = m.TMdbId,
                    Type = m.Type,
                    Title = m.Title,
                    Year = m.Year,
                    Overview = m.Overview,
                    PosterUrl = m.PosterUrl,
                    Genres = m.Genres
                }).ToListAsync();

            _logger.LogInformation("Получено {Count} фильмов из базы данных", movies.Count);
            return movies;
        }

        public async Task<BaseResponseWithDataDto<MovieDto>> GetMovieByIdAsync(int id)
        {
            var movie = await _db.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                _logger.LogInformation("Фильм с ID {Id} не найден", id);
                return new BaseResponseWithDataDto<MovieDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.NotFound,
                    ResponseMessage = "Movie not found."
                };
            }

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

            _logger.LogInformation("Фильм с ID {Id}найден", id);
            return new BaseResponseWithDataDto<MovieDto>
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "Movie retrieved successfully.",
                Data = response
            };
        }

        public async Task<BaseResponseDto> DeleteMovieAsync(int id)
        {
            var movie = await _db.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                _logger.LogInformation("Фильм с ID {id} не найден", id);
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.NotFound,
                    ResponseMessage = "Movie not found."
                };
            }

            try
            {
                _db.Movies.Remove(movie);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении фильма с ID {id}", id);
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.ServerError,
                    ResponseMessage = "An error occurred while deleting the movie."
                };
            }

            _logger.LogInformation("Фильм с TMDb ID {id} удален", id);
            return new BaseResponseWithDataDto<MovieDto>
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "Movie has been deleted."
            };
        }
    }
}
