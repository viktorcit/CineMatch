using CineMatch.Api.Data;
using CineMatch.Api.Data.Contracts;
using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Entity;
using CineMatch.Api.Enums;
using CineMatch.Api.Helpers;
using CineMatch.Api.Services.Interfaces.IMovieServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CineMatch.Api.Services.MovieServices
{
    public class MovieService : IMovieService
    {
        private readonly ILogger<MovieService> _logger;
        private readonly AppDbContext _db;
        private readonly IMovieSearchService _movieSearchService;
        private readonly ITmdbService _tmdbService;
        public MovieService(
            ILogger<MovieService> logger,
            AppDbContext db,
            IMovieSearchService movieSearchService,
            ITmdbService tmdbService)
        {
            _logger = logger;
            _db = db;
            _movieSearchService = movieSearchService;
            _tmdbService = tmdbService;
        }


        public async Task<BaseResponseDto> SaveMovieAsync(int tmdbId, ContentType type, string userId)
        {
            _logger.LogInformation("Сохранение фильма");

            var SessionParticipant = await _db.SessionParticipants
                .FirstOrDefaultAsync(sp => sp.UserId == userId);
            if (SessionParticipant == null)
            {
                _logger.LogInformation("Сессия клиента не найдена для Client ID {ClientId} либо был завершена", userId);
                return ResponseFactory.Fail(ErrorType.NotFound, "You are not in any session and cannot save movies or session closed.");
            }

            var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == SessionParticipant.SessionId);
            if (session == null)
            {
                return ResponseFactory.Fail(ErrorType.NotFound, "Session not found for the client.");
            }

            var movieExists = await _db.Movies.FirstOrDefaultAsync(m => m.TMdbId == tmdbId && m.Type == type);
            if (movieExists != null)
            {
                var movieExistSession = await _db.SessionMovies.FirstOrDefaultAsync(sm => sm.MovieId == movieExists.Id && sm.SessionId == session.Id);
                if (movieExistSession != null)
                {
                    _logger.LogInformation("Фильм уже добавлен в сессию и есть в базе данных");
                    return ResponseFactory.Fail(ErrorType.Conflict, "Movie with the same TMDb ID and type already exists in your session");
                }
                else if(movieExistSession == null)
                {
                    _logger.LogInformation("Фильм уже существует в базе данных, но не добавлен в сессию. Добавляем фильм в сессию.");
                    var sessionMovie = CreateSessionMovieEntity(session, movieExists);
                    _db.SessionMovies.Add(sessionMovie);
                    await _db.SaveChangesAsync();

                    return ResponseFactory.Ok("Movie has been added to the session.");
                }

                _logger.LogInformation("Фильм уже существует в базе данных");
                return ResponseFactory.Fail(ErrorType.Conflict, "Movie with the same TMDb ID and type already exists.");
            }

            try
            {
                var movie = await GetMovieDataByIdAsync(tmdbId, type);
                if (movie == null)
                {
                    return ResponseFactory.Fail(ErrorType.NotFound, "Movie not found with same TMDb Id");
                }
                await AddFilmInDbAndSession(movie, session);
            }
            catch (Exception ex)
            {
                _logger.LogError("Не удалось добавить фильм в бд и сессию: {ex}", ex);
                return ResponseFactory.Fail(ErrorType.ServerError, ResponseMessages.ServerError);
            }

            return ResponseFactory.Ok("Movie saved successfully.");
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
                return ResponseFactory.Fail<MovieInfo>(ErrorType.NotFound, "Movie not found.");
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
            return ResponseFactory.Ok(response);
        }

        [Authorize] //TODO admin only can delete movie from DB
        public async Task<BaseResponseDto> DeleteMovieAsync(int id)
        {
            var movie = await _db.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                _logger.LogInformation("Фильм с ID {id} не найден", id);
                return ResponseFactory.Fail(ErrorType.NotFound, "Movie not found.");
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
                return ResponseFactory.Fail(ErrorType.ServerError, ResponseMessages.ServerError);
            }

            _logger.LogInformation("Фильм с TMDb ID {id} удален", id);
            return ResponseFactory.Fail(ErrorType.NoContent, "Movie has been deleted.");
        }

        //private methods
        private async Task AddFilmInDbAndSession(Movie movie, Session session)
        {
            var sessionMovie = CreateSessionMovieEntity(session, movie);
            await _db.Movies.AddAsync(movie);
            await _db.SessionMovies.AddAsync(sessionMovie);
            await _db.SaveChangesAsync();
        }

        private async Task<Movie?> GetMovieDataByIdAsync(int tmdbId, ContentType type)
        {
            var movie = await _db.Movies.FirstOrDefaultAsync(m => m.TMdbId == tmdbId && m.Type == type);
            if (movie == null)
            {
                movie = await FetchMovieDataFromApiAsync(tmdbId, type);
                if (movie == null)
                {
                    return null;
                }
            }
            var response = CreateMovieDto(movie);
            return movie;
        }

        private async Task<Movie?> FetchMovieDataFromApiAsync(int tmdbId, ContentType type)
        {
            var movieData = await _tmdbService.GetMovieDetailsAsync(tmdbId, type);
            if (movieData == null)
            {
                return null;
            }
            var movie = CreateMovieEntity(movieData);
            return movie;
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

        private static MovieInfo CreateMovieDto(Movie movie)
        {
            var dto = new MovieInfo
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
            return dto;
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
