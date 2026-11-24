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
        private readonly ITmdbService _tmdbService;
        public MovieService(
            ILogger<MovieService> logger,
            AppDbContext db,
            ITmdbService tmdbService)
        {
            _logger = logger;
            _db = db;
            _tmdbService = tmdbService;
        }


        public async Task<BaseResponseDto> SaveMovieAsync(int tmdbId, ContentType type, string userId, CancellationToken cancellationToken)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "SaveMovieAsync", ["UserId"] = userId });


            var SessionParticipant = await _db.SessionParticipants
                .FirstOrDefaultAsync(sp => sp.UserId == userId, 
                cancellationToken);
            if (SessionParticipant == null)
            {
                _logger.LogDebug("Operation rejected: {Reason}", "You are not in any session and cannot save movies or session closed.");
                return ResponseFactory.Fail(ErrorType.NotFound, "You are not in any session and cannot save movies or session closed.");
            }

            var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == SessionParticipant.SessionId,
                cancellationToken);
            if (session == null)
            {
                _logger.LogDebug("Operation rejected: {Reason}", "Session not found for the client.");
                return ResponseFactory.Fail(ErrorType.NotFound, "Session not found for the client.");
            }

            var movieExists = await _db.Movies.FirstOrDefaultAsync(m => m.TMdbId == tmdbId && m.Type == type,
                cancellationToken);
            if (movieExists != null)
            {
                var movieExistSession = await _db.SessionMovies.FirstOrDefaultAsync(sm => sm.MovieId == movieExists.Id && sm.SessionId == session.Id,
                    cancellationToken);
                if (movieExistSession != null)
                {
                    _logger.LogDebug("Operation rejected: {Reason}", "Movie with the same TMDb ID and type already exists in your session");
                    return ResponseFactory.Fail(ErrorType.Conflict, "Movie with the same TMDb ID and type already exists in your session");
                }
                else if(movieExistSession == null)
                {
                    var sessionMovie = CreateSessionMovieEntity(session, movieExists);
                    _db.SessionMovies.Add(sessionMovie);
                    await _db.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Existing movie {MovieId} added to session {SessionId} by user {UserId}", movieExists.Id, session.Id, userId);
                    return ResponseFactory.Ok("Movie has been added to the session.");
                }

                _logger.LogDebug("Operation rejected: {Reason}", "Movie with the same TMDb ID and type already exists.");
                return ResponseFactory.Fail(ErrorType.Conflict, "Movie with the same TMDb ID and type already exists.");
            }

            try
            {
                var movie = await GetMovieDataByIdAsync(tmdbId, type, cancellationToken);
                if (movie == null)
                {
                    _logger.LogDebug("Operation rejected: {Reason}", "Movie not found with same TMDb Id");
                    return ResponseFactory.Fail(ErrorType.NotFound, "Movie not found with same TMDb Id");
                }
                await AddFilmInDbAndSessionAsync(movie, session, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Log(ex is OperationCanceledException && cancellationToken.IsCancellationRequested ? LogLevel.Debug : LogLevel.Error, ex, "Failed to save TMDb item {TmdbId} ({ContentType}) in session {SessionId} for user {UserId}", tmdbId, type, session.Id, userId);
                return ResponseFactory.Fail(ErrorType.ServerError, ResponseMessages.ServerError);
            }

            _logger.LogInformation("TMDb item {TmdbId} ({ContentType}) saved in session {SessionId} by user {UserId}", tmdbId, type, session.Id, userId);
            return ResponseFactory.Ok("Movie saved successfully.");
        }

        public async Task<List<MovieInfo>> GetAllMoviesAsync(CancellationToken cancellationToken)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "GetAllMoviesAsync" });
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
                }).ToListAsync(cancellationToken);

            _logger.LogDebug("Read {MovieCount} movies from the catalogue", movies.Count);
            return movies;
        }

        public async Task<BaseResponseDto<MovieInfo>> GetMovieByIdAsync(int id)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "GetMovieByIdAsync" });
            var movie = await _db.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                _logger.LogDebug("Operation rejected: {Reason}", "Movie not found.");
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

            return ResponseFactory.Ok(response);
        }


        public async Task<BaseResponseDto> DeleteMovieAsync(int id)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "DeleteMovieAsync" });
            var movie = await _db.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                _logger.LogDebug("Operation rejected: {Reason}", "Movie not found.");
                return ResponseFactory.Fail(ErrorType.NotFound, "Movie not found.");
            }

            try
            {
                _db.Movies.Remove(movie);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete movie {MovieId} from the catalogue", id);
                return ResponseFactory.Fail(ErrorType.ServerError, ResponseMessages.ServerError);
            }

            _logger.LogInformation("Movie {MovieId} deleted from the catalogue", id);
            return ResponseFactory.Fail(ErrorType.NoContent, "Movie has been deleted.");
        }

        //private methods
        private async Task AddFilmInDbAndSessionAsync(Movie movie, Session session, CancellationToken cancellationToken)
        {
            var sessionMovie = CreateSessionMovieEntity(session, movie);
            await _db.Movies.AddAsync(movie, cancellationToken);
            await _db.SessionMovies.AddAsync(sessionMovie, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        private async Task<Movie?> GetMovieDataByIdAsync(int tmdbId, ContentType type, CancellationToken cancellationToken)
        {
            var movie = await _db.Movies.FirstOrDefaultAsync(m => m.TMdbId == tmdbId && m.Type == type);
            if (movie == null)
            {
                movie = await FetchMovieDataFromApiAsync(tmdbId, type, cancellationToken);
                if (movie == null)
                {
                    return null;
                }
            }
            var response = CreateMovieDto(movie);
            return movie;
        }

        private async Task<Movie?> FetchMovieDataFromApiAsync(int tmdbId, ContentType type, CancellationToken cancellationToken)
        {
            var movieData = await _tmdbService.GetMovieDetailsAsync(tmdbId, type, cancellationToken);
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
