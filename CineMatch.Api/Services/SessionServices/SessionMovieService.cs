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



        public async Task<BaseResponseDto<List<MovieInfo>>> GetFilmsOfSessionAsync(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return ErrorFactory.Fail<List<MovieInfo>>(ErrorType.BadRequest, "Client ID cannot be empty");
            }
            var clientSession = await _db.SessionParticipants
                .FirstOrDefaultAsync(p => p.ClientId == clientId);
            if (clientSession == null)
            {
                return ErrorFactory.Fail<List<MovieInfo>>(ErrorType.BadRequest, "You are not a participant of any session");
            }

            var sessionMovies = await FindSessionMovies(clientSession.SessionId);
            if (sessionMovies == null || sessionMovies.Count == 0)
            {
                return ErrorFactory.Fail<List<MovieInfo>>(ErrorType.NoContent, "No movies found for this session");
            }

            return ErrorFactory.Ok(sessionMovies, "Films retrieved successfully");
        }

        public async Task<BaseResponseDto<List<MovieInfo>>> GetMatchedInSessionMovieAsync(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return ErrorFactory.Fail<List<MovieInfo>>(ErrorType.BadRequest, "Client ID cannot be empty");
            }

            var clientParticipant = await _db.SessionParticipants
                .FirstOrDefaultAsync(p => p.ClientId == clientId);
            if (clientParticipant == null)
            {
                return ErrorFactory.Fail<List<MovieInfo>>(ErrorType.Conflict, "You are not a participant of any session");
            }

            var sessionId = clientParticipant.SessionId;
            var clientSession = await _db.Sessions
                .FirstOrDefaultAsync(s => s.Id == sessionId);
            if (clientSession == null)
            {
                return ErrorFactory.Fail<List<MovieInfo>>(ErrorType.NotFound, "Session not found");
            }

            var matchedMovieIds = await GetMatchedMoviesIds(sessionId);
            if (matchedMovieIds == null || matchedMovieIds.Count == 0)
            {
                return ErrorFactory.Fail<List<MovieInfo>>(ErrorType.NoContent, "No matched movies found for this session");
            }

            var matchedMovies = await CreateMatchedMoviesList(matchedMovieIds);
            if (matchedMovies == null || matchedMovies.Count == 0)
            {
                return ErrorFactory.Fail<List<MovieInfo>>(ErrorType.NoContent, "No matched movies found for this session");
            }


            return ErrorFactory.Ok(matchedMovies, "Matched movies retrieved successfully");
        }

        public async Task<BaseResponseDto<MovieInfo>> GetRandomMatchedFilmAsync(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                return ErrorFactory.Fail<MovieInfo>(ErrorType.BadRequest, "Client ID cannot be empty");
            }

            var clientParticipant = await _db.SessionParticipants
                .FirstOrDefaultAsync(p => p.ClientId == clientId);
            if (clientParticipant == null)
            {
                return ErrorFactory.Fail<MovieInfo>(ErrorType.Conflict, "You are not a participant of any session");
            }

            var sessionId = clientParticipant.SessionId;
            var matchedMovieIds = await GetMatchedMoviesIds(sessionId);
            if (matchedMovieIds == null || matchedMovieIds.Count == 0)
            {
                return ErrorFactory.Fail<MovieInfo>(ErrorType.NoContent, "No matched movies found for this session");
            }

            var randomMovie = await RandomMovie(matchedMovieIds);
            if (randomMovie == null)
            {
                return ErrorFactory.Fail<MovieInfo>(ErrorType.NotFound, "Matched movie not found");
            }

            var response = CreateMovieDto(randomMovie);

            return ErrorFactory.Ok(response, "Random matched movie retrieved successfully");
        }

        //private methods
        private async Task<List<MovieInfo>> FindSessionMovies(int sessionId)
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
                }).ToListAsync();
            return sessionMovies;
        }

        private async Task<List<int>> GetMatchedMoviesIds(int sessionId)
        {
            var matchedMovieIds = await _db.Votes
                .Where(v => v.SessionId == sessionId && v.IsLiked)
                .GroupBy(v => v.MovieId)
                .Where(g => g.Select(v => v.ParticipantNumber).Distinct().Count() == 2)
                .Select(g => g.Key)
                .ToListAsync();
            return matchedMovieIds;
        }

        private async Task<List<MovieInfo>> CreateMatchedMoviesList(List<int> matchedMovieIds)
        {
            List<MovieInfo> matchedMovies = [];

            foreach (var movieId in matchedMovieIds)
            {
                var movie = await _db.Movies.FirstOrDefaultAsync(m => m.Id == movieId);
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

        private async Task<Movie?> RandomMovie(List<int> matchedMovieIds)
        {
            var random = new Random();
            var randomMovieId = matchedMovieIds[random.Next(matchedMovieIds.Count)];
            var movie = await _db.Movies.FirstOrDefaultAsync(m => m.Id == randomMovieId);
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
