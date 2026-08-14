using CineMatch.Api.Data;
using CineMatch.Api.Data.Contracts;
using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Entity;
using CineMatch.Api.Enums;
using CineMatch.Api.Helpers;
using CineMatch.Api.Services.Interfaces.IMovieServices;
using Microsoft.EntityFrameworkCore;

namespace CineMatch.Api.Services.MovieServices
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


        public async Task<BaseResponseDto> SaveMovieAsync(MovieInfo dto, string userId)
        {
            _logger.LogInformation("Сохранение фильма");
            if (dto == null)
            {
                _logger.LogInformation("Нет данных для сохранения");
                return ErrorFactory.Fail(ErrorType.BadRequest ,"Movie data cannot be null.");
            }
            if (string.IsNullOrEmpty(dto.Title))
            {
                _logger.LogInformation("Название фильма не указано");
                return ErrorFactory.Fail(ErrorType.BadRequest, "Movie title is required.");
            }
            if (dto.TMdbId <= 0)
            {
                _logger.LogInformation("Некорректный TMDb ID");
                return ErrorFactory.Fail(ErrorType.BadRequest, "TMDb ID must be a positive integer.");
            }
            if (dto.Year.HasValue && (dto.Year < 1888 || dto.Year > DateTime.Now.Year + 1))
            {
                _logger.LogInformation("Некорректный год выпуска");
                return ErrorFactory.Fail(ErrorType.BadRequest, "TMDb ID must be a positive integer.");
            }
            if (string.IsNullOrEmpty(userId))
            {
                return ErrorFactory.Fail(ErrorType.BadRequest, "User ID cannot be null");
            }

            var SessionParticipant = await _db.SessionParticipants
                .FirstOrDefaultAsync(sp => sp.UserId == userId);
            if (SessionParticipant == null)
            {
                _logger.LogInformation("Сессия клиента не найдена для Client ID {ClientId} либо был завершена", userId);
                return ErrorFactory.Fail(ErrorType.NotFound, "You are not in any session and cannot save movies or session closed.");
            }

            var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == SessionParticipant.SessionId);
            if (session == null)
            {
                return ErrorFactory.Fail(ErrorType.NotFound, "Session not found for the client.");
            }

            var movieExists = await _db.Movies.FirstOrDefaultAsync(m => m.TMdbId == dto.TMdbId && m.Type == dto.Type);
            if (movieExists != null)
            {
                var movieExistSession = await _db.SessionMovies.FirstOrDefaultAsync(sm => sm.MovieId == movieExists.Id && sm.SessionId == session.Id);
                if (movieExistSession != null)
                {
                    _logger.LogInformation("Фильм уже добавлен в сессию и есть в базе данных");
                    return ErrorFactory.Fail(ErrorType.Conflict, "Movie with the same TMDb ID and type already exists in your session");
                }
                else if(movieExistSession == null)
                {
                    _logger.LogInformation("Фильм уже существует в базе данных, но не добавлен в сессию. Добавляем фильм в сессию.");
                    var sessionMovie = CreateSessionMovieEntity(session, movieExists);
                    _db.SessionMovies.Add(sessionMovie);
                    await _db.SaveChangesAsync();

                    return ErrorFactory.Ok("Movie has been added to the session.");
                }

                _logger.LogInformation("Фильм уже существует в базе данных");
                return ErrorFactory.Fail(ErrorType.Conflict, "Movie with the same TMDb ID and type already exists.");
            }

            try
            {
                var addFilm = await AddFilmInDbAndSession(dto, session);
            }
            catch (Exception ex)
            {
                _logger.LogError("Не удалось добавить фильм в бд и сессию: {ex}", ex);
                return ErrorFactory.Fail(ErrorType.ServerError, ResponseMessages.ServerError);
            }

            return ErrorFactory.Ok("Movie saved successfully.");
        }

        public async Task<List<MovieInfo>> GetAllMoviesAsync()
        {
            var movies = await _db.Movies
                .Select(m => new MovieInfo
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

        public async Task<BaseResponseDto<MovieInfo>> GetMovieByIdAsync(int id)
        {
            var movie = await _db.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                _logger.LogInformation("Фильм с ID {Id} не найден", id);
                return ErrorFactory.Fail<MovieInfo>(ErrorType.NotFound, "Movie not found.");
            }

            var response = new MovieInfo
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

            _logger.LogInformation("Фильм с ID {Id}найден", movie.Id);
            return ErrorFactory.Ok(response, "Movie retrieved successfully.");
        }

        public async Task<BaseResponseDto> DeleteMovieAsync(int id)
        {
            var movie = await _db.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                _logger.LogInformation("Фильм с ID {id} не найден", id);
                return ErrorFactory.Fail(ErrorType.NotFound, "Movie not found.");
            }

            try
            {
                _db.Movies.Remove(movie);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                List<string> errorMessage = [ex.Message];
                _logger.LogError(ex, "Ошибка при удалении фильма с ID {id}", id);
                return ErrorFactory.Fail(ErrorType.ServerError, ResponseMessages.ServerError);
            }

            _logger.LogInformation("Фильм с TMDb ID {id} удален", id);
            return ErrorFactory.Fail(ErrorType.NoContent, "Movie has been deleted.");
        }

        //private methods
        private async Task<bool> AddFilmInDbAndSession(MovieInfo dto, Session session)
        {
            var movie = CreateMovieEntity(dto);
            var sessionMovieTwo = CreateSessionMovieEntity(session, movie);

            await _db.Movies.AddAsync(movie);
            await _db.SessionMovies.AddAsync(sessionMovieTwo);
            await _db.SaveChangesAsync();
            return true;
        }

        //static private methods
        private static Movie CreateMovieEntity(MovieInfo dto)
        {
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
            return movie;
        }

        private static SessionMovie CreateSessionMovieEntity(Session session, Movie movie)
        {
            var sessionMovie = new SessionMovie
            {
                SessionId = session.Id,
                MovieId = movie.Id,
                Movie = movie
            };
            return sessionMovie;
        }
    }
}
