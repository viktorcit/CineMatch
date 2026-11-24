using CineMatch.Api.Data;
using CineMatch.Api.Data.Contracts;
using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Entity;
using CineMatch.Api.Enums;
using CineMatch.Api.Helpers;
using CineMatch.Api.Services.Interfaces.ISessionServices;
using Microsoft.EntityFrameworkCore;

namespace CineMatch.Api.Services.SessionServices
{
    public class SessionMovieService : ISessionMovieService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<SessionMovieService> _logger;

        public SessionMovieService(AppDbContext db, ILogger<SessionMovieService> logger)
        {
            _db = db;
            _logger = logger;
        }


        public async Task<BaseResponseDto<List<MovieInfo>>> GetFilmsOfSessionAsync(string sessionCode, string userId, CancellationToken cancellationToken)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "GetFilmsOfSessionAsync", ["UserId"] = userId });
            var session = await UserPartisipatingAsync(sessionCode, userId, cancellationToken);
            if (session == null)
            {
                _logger.LogDebug("Operation rejected: {Reason}", "Session not found for this code, or you are not a participant.");
                return ResponseFactory.Fail<List<MovieInfo>>(ErrorType.Conflict, "Session not found for this code, or you are not a participant.");
            }

            var sessionMovies = await FindSessionMoviesAsync(session.Id, cancellationToken);
            if (sessionMovies == null || sessionMovies.Count == 0)
            {
                _logger.LogDebug("Operation rejected: {Reason}", "No movies found for this session");
                return ResponseFactory.Fail<List<MovieInfo>>(ErrorType.NoContent, "No movies found for this session");
            }

            return ResponseFactory.Ok(sessionMovies);
        }

        public async Task<BaseResponseDto<List<MovieInfo>>> GetMatchedInSessionMovieAsync(string sessionCode, string userId, CancellationToken cancellationToken)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "GetMatchedInSessionMovieAsync", ["UserId"] = userId });
            var session = await UserPartisipatingAsync(sessionCode, userId, cancellationToken);
            if (session == null)
            {
                _logger.LogDebug("Operation rejected: {Reason}", "Session not found for this code, or you are not a participant.");
                return ResponseFactory.Fail<List<MovieInfo>>(ErrorType.Conflict, "Session not found for this code, or you are not a participant.");
            }

            var matchedMovieIds = await GetMatchedMoviesIdsAsync(session.Id, cancellationToken);
            if (matchedMovieIds == null || matchedMovieIds.Count == 0)
            {
                _logger.LogDebug("Operation rejected: {Reason}", "No matched movies found for this session");
                return ResponseFactory.Fail<List<MovieInfo>>(ErrorType.NoContent, "No matched movies found for this session");
            }

            var matchedMovies = await CreateMatchedMoviesListAsync(matchedMovieIds, cancellationToken);
            if (matchedMovies == null || matchedMovies.Count == 0)
            {
                _logger.LogDebug("Operation rejected: {Reason}", "No matched movies found for this session");
                return ResponseFactory.Fail<List<MovieInfo>>(ErrorType.NoContent, "No matched movies found for this session");
            }

            return ResponseFactory.Ok(matchedMovies);
        }

        public async Task<BaseResponseDto<MovieInfo>> GetRandomMatchedFilmAsync(string sessionCode, string userId, CancellationToken cancellationToken)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "GetRandomMatchedFilmAsync", ["UserId"] = userId });
            var session = await UserPartisipatingAsync(sessionCode, userId, cancellationToken);
            if (session == null)
            {
                _logger.LogDebug("Operation rejected: {Reason}", "Session not found for this code, or you are not a participant.");
                return ResponseFactory.Fail<MovieInfo>(ErrorType.Conflict, "Session not found for this code, or you are not a participant.");
            }

            var matchedMovieIds = await GetMatchedMoviesIdsAsync(session.Id, cancellationToken);
            if (matchedMovieIds == null || matchedMovieIds.Count == 0)
            {
                _logger.LogDebug("Operation rejected: {Reason}", "No matched movies found for this session");
                return ResponseFactory.Fail<MovieInfo>(ErrorType.NoContent, "No matched movies found for this session");
            }

            var randomMovie = await RandomMovieAsync(matchedMovieIds, cancellationToken);
            if (randomMovie == null)
            {
                _logger.LogDebug("Operation rejected: {Reason}", "Matched movie not found");
                return ResponseFactory.Fail<MovieInfo>(ErrorType.NotFound, "Matched movie not found");
            }

            var response = CreateMovieDto(randomMovie);

            return ResponseFactory.Ok(response);
        }

        public async Task<BaseResponseDto> DeleteMovieFromSessionAsync(int movieId, string userId)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "DeleteMovieFromSessionAsync", ["UserId"] = userId });
            var sessionParticipant = await _db.SessionParticipants.FirstOrDefaultAsync(sp => sp.UserId == userId);
            if (sessionParticipant == null)
            {
                _logger.LogDebug("Operation rejected: {Reason}", "You are not a participant in any session.");
                return ResponseFactory.Fail(ErrorType.Conflict, "You are not a participant in any session.");
            }
            var sessionMovie = await _db.SessionMovies.FirstOrDefaultAsync(m => m.Id == movieId && m.SessionId == sessionParticipant.SessionId);
            if (sessionMovie == null)
            {
                _logger.LogDebug("Operation rejected: {Reason}", "Movie not found from this session.");
                return ResponseFactory.Fail(ErrorType.NotFound, "Movie not found from this session.");
            }

            try
            {
                _db.SessionMovies.Remove(sessionMovie);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove session movie {SessionMovieId} from session {SessionId} for user {UserId}", movieId, sessionParticipant.SessionId, userId);
                return ResponseFactory.Fail(ErrorType.ServerError, ResponseMessages.ServerError);
            }
            _logger.LogInformation("Session movie {SessionMovieId} removed from session {SessionId} by user {UserId}", movieId, sessionParticipant.SessionId, userId);
            return ResponseFactory.Ok("Movie retrieved successfully.");
        }

        //private methods
        private async Task<List<MovieInfo>> FindSessionMoviesAsync(int sessionId, CancellationToken cancellationToken)
        {
            var sessionMovies = await _db.SessionMovies
                .Where(sm => sm.SessionId == sessionId)
                .Select(sm => new MovieInfo
                {
                    Id = sm.MovieId,
                    TMdbId = sm.Movie.TMdbId,
                    Type = sm.Movie.Type,
                    Title = sm.Movie.Title,
                    Year = sm.Movie.Year,
                    Overview = sm.Movie.Overview,
                    PosterUrl = sm.Movie.PosterUrl,
                    Genres = sm.Movie.Genres
                }).ToListAsync(cancellationToken);
            return sessionMovies;
        }

        private async Task<List<int>> GetMatchedMoviesIdsAsync(int sessionId, CancellationToken cancellationToken)
        {
            var matchedMovieIds = await _db.Votes
                .Where(v => v.SessionId == sessionId && v.IsLiked)
                .GroupBy(v => v.MovieId)
                .Where(g => g.Select(v => v.ParticipantId).Distinct().Count() == 2)
                .Select(g => g.Key)
                .ToListAsync(cancellationToken);
            return matchedMovieIds;
        }

        private async Task<List<MovieInfo>> CreateMatchedMoviesListAsync(List<int> matchedMovieIds, CancellationToken cancellationToken)
        {
            List<MovieInfo> matchedMovies = [];

            foreach (var movieId in matchedMovieIds)
            {
                var movie = await _db.Movies.FirstOrDefaultAsync(m => m.Id == movieId, cancellationToken);
                if (movie != null)
                {
                    matchedMovies.Add(new MovieInfo
                    {
                        Id = movie.Id,
                        TMdbId = movie.TMdbId,
                        Type = movie.Type,
                        Title = movie.Title,
                        Year = movie.Year,
                        Overview = movie.Overview,
                        PosterUrl = movie.PosterUrl,
                        Genres = movie.Genres,
                    });
                }
            }
            return matchedMovies;
        }

        private async Task<Session?> UserPartisipatingAsync(string sessionCode, string userId, CancellationToken cancellationToken)
        {
            var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Code == sessionCode, cancellationToken);
            if (session == null)
            {
                return null;
            }

            var sessionParticipant = await _db.SessionParticipants.FirstOrDefaultAsync(sp => sp.UserId == userId && sp.SessionId == session.Id, cancellationToken);
            if (sessionParticipant == null)
            {
                return null;
            }
            return session;
        }

        private async Task<Movie?> RandomMovieAsync(List<int> matchedMovieIds, CancellationToken cancellationToken)
        {
            var random = new Random();
            var randomMovieId = matchedMovieIds[random.Next(matchedMovieIds.Count)];
            var movie = await _db.Movies.FirstOrDefaultAsync(m => m.Id == randomMovieId, cancellationToken);
            return movie;
        }


        //private static methods
        private static MovieInfo CreateMovieDto(Movie movie)
        {
            var response = new MovieInfo
            {
                Id = movie.Id,
                TMdbId = movie.TMdbId,
                Type = movie.Type,
                Title = movie.Title,
                Year = movie.Year,
                Overview = movie.Overview,
                PosterUrl = movie.PosterUrl,
                Genres = movie.Genres,
            };
            return response;

        }
    }
}
