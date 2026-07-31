using CineMatch.Api.Data;
using CineMatch.Api.Data.DTO;
using CineMatch.Api.Data.DTO.MoviesDto;
using CineMatch.Api.Data.DTO.SessionDto;
using CineMatch.Api.Enums;
using CineMatch.Api.Helpers;
using CineMatch.Api.Model;
using CineMatch.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineMatch.Api.Services.UserServices
{
    public class SessionService : ISessionService
    {
        private readonly ILogger<SessionService> _logger;
        private readonly AppDbContext _db;

        public SessionService(ILogger<SessionService> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }



        public async Task<BaseResponseDto<SessionDto>> CreateSessionAsync(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return ErrorFactory.Fail<SessionDto>(ErrorType.BadRequest, "Client ID cannot be empty");
            }
            _logger.LogInformation("создание сессии");
            var existingSession = await _db.Sessions
                .AnyAsync(s => s.CreatorClientId == clientId);
            if (existingSession)
            {
                return ErrorFactory.Fail<SessionDto>(ErrorType.BadRequest, "You already have an active session.");
            }

            var existingParticipant = await _db.SessionParticipants
                .AnyAsync(p => p.ClientId == clientId);
            if (existingParticipant)
            {
                return ErrorFactory.Fail<SessionDto>(ErrorType.Conflict, "You are already a participant of another session. Leave for creation");
            }

            var session = new Session
            {
                Code = await GenerateCode(),
                CreatedAt = DateTime.UtcNow,
                CreatorClientId = clientId,
            };

            var participant = new SessionParticipant
            {
                ClientId = clientId,
                Session = session,
                SessionId = session.Id,
                ParticipantNumber = 1,
            };

            _db.SessionParticipants.Add(participant);
            _db.Sessions.Add(session);
            await _db.SaveChangesAsync();

            var response = new SessionDto
            {
                Id = session.Id,
                Code = session.Code,
                CreatedAt = session.CreatedAt,
                CreatorClientId = session.CreatorClientId,
                SessionMovies = [],
                Votes = [],
            };


            return ErrorFactory.Ok(response, "Session created successfully");
        }


        public async Task<BaseResponseDto> JoinToSessionAsync(string code, string clientId)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return ErrorFactory.Fail(ErrorType.BadRequest, "Session code cannot be empty");
            }

            var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Code == code);
            if (session == null)
            {
                return ErrorFactory.Fail(ErrorType.NotFound, "Session not found");
            }

            var alreadyJoined = await _db.SessionParticipants
                .FirstOrDefaultAsync(p => p.ClientId == clientId && p.SessionId == session.Id);
            if (alreadyJoined != null)
            {
                return ErrorFactory.Fail(ErrorType.Conflict, "You are already a participant of this session");
            }

            var existingParticipant = await _db.SessionParticipants.AnyAsync(p => p.ClientId == clientId);
            if (existingParticipant)
            {
                return ErrorFactory.Fail(ErrorType.Conflict, "You are already a participant of another session");
            }

            var sessionParticipants = await _db.SessionParticipants
                .Where(p => p.SessionId == session.Id).ToListAsync();
            if (sessionParticipants.Count() >= 2)
            {
                return ErrorFactory.Fail(ErrorType.Conflict, "Session is full");
            }

            var participantNumber = sessionParticipants.Count + 1;

            var newParticipant = new SessionParticipant
            {
                ClientId = clientId,
                SessionId = session.Id,
                Session = session,
                ParticipantNumber = participantNumber,
            };

            _db.SessionParticipants.Add(newParticipant);
            await _db.SaveChangesAsync();

            return ErrorFactory.Ok("Joined session successfully");
        }


        public async Task<BaseResponseDto<List<MovieDto>>> GetFilmsOfSessionAsync(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return ErrorFactory.Fail<List<MovieDto>>(ErrorType.BadRequest, "Client ID cannot be empty");
            }
            var clientSession = await _db.SessionParticipants
                .FirstOrDefaultAsync(p => p.ClientId == clientId);
            if (clientSession == null)
            {
                return ErrorFactory.Fail<List<MovieDto>>(ErrorType.BadRequest, "You are not a participant of any session");
            }
            var sessionMovies = await _db.SessionMovies
                .Where(sm => sm.SessionId == clientSession.SessionId)
                .Select(sm => new MovieDto
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

            if (sessionMovies == null || sessionMovies.Count == 0)
            {
                return ErrorFactory.Fail<List<MovieDto>>(ErrorType.NoContent, "No movies found for this session");
            }


            return ErrorFactory.Ok(sessionMovies, "Films retrieved successfully");
        }


        public async Task<BaseResponseDto> LeaveSessionAsync(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return ErrorFactory.Fail(ErrorType.BadRequest, "Client ID cannot be empty");
            }

            var sessionParticipant = await _db.SessionParticipants
                .FirstOrDefaultAsync(p => p.ClientId == clientId);
            if (sessionParticipant == null)
            {
                return ErrorFactory.Fail(ErrorType.Conflict, "You are not a participant in any session");
            }
            var sessionCreatorExist = await _db.Sessions
                .AnyAsync(s => s.Id == sessionParticipant.SessionId && s.CreatorClientId == clientId);
            if (sessionCreatorExist)
            {
                return ErrorFactory.Fail(ErrorType.Conflict, "You are a creator of this session. End session for leaving");
            }

            _db.SessionParticipants.Remove(sessionParticipant);
            await _db.SaveChangesAsync();

            return ErrorFactory.Ok("Left session successfully");
        }

        public async Task<BaseResponseDto> EndSessionAsync(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return ErrorFactory.Fail(ErrorType.BadRequest, "Client ID cannot be empty");
            }
            var sessionCreator = await _db.Sessions
                .FirstOrDefaultAsync(p => p.CreatorClientId == clientId);
            if (sessionCreator == null)
            {
                return ErrorFactory.Fail(ErrorType.Conflict, "You don't have an active session that you created to end it.");
            }
            _db.Sessions.Remove(sessionCreator);
            await _db.SaveChangesAsync();
            return ErrorFactory.Ok("Session ended successfully");
        }

        public async Task<BaseResponseDto> LikeFilmsAsync(string clientId, int? movieId)
        {
            if (string.IsNullOrWhiteSpace(clientId) || !movieId.HasValue)
            {
                return ErrorFactory.Fail(ErrorType.BadRequest, "Client ID and movie ID cannot be empty");
            }

            var clientParticipant = await _db.SessionParticipants
                .FirstOrDefaultAsync(p => p.ClientId == clientId);
            if (clientParticipant == null)
            {
                return ErrorFactory.Fail(ErrorType.Conflict, "You are not a participant of any session");
            }

            var sessionId = clientParticipant.SessionId;
            var clientSession = await _db.Sessions
                .FirstOrDefaultAsync(s => s.Id == sessionId);
            if (clientSession == null)
            {
                return ErrorFactory.Fail(ErrorType.NotFound, "Session not found");
            }

            var sessionMovie = await _db.SessionMovies
                .FirstOrDefaultAsync(sm => sm.SessionId == sessionId && sm.MovieId == movieId);
            if (sessionMovie == null)
            {
                return ErrorFactory.Fail(ErrorType.NotFound, "Movie not found in this session");
            }
            var participantNumber = clientParticipant.ParticipantNumber;
            _logger.LogInformation("участник {participantNumber}", participantNumber);
            if (participantNumber != 1 && participantNumber != 2)
            {
                return ErrorFactory.Fail(ErrorType.Conflict, "Invalid participant number");
            }

            var existingVote = await _db.Votes
            .AnyAsync(v => v.ParticipantNumber == participantNumber && v.SessionId == sessionId && v.MovieId == movieId);
            if (existingVote)
            {
                return ErrorFactory.Fail(ErrorType.Conflict, "You have already voted for this session");
            }

            var vote = new Vote
            {
                IsLiked = true,
                SessionId = sessionId,
                ParticipantNumber = participantNumber,
                MovieId = movieId.Value,
                Session = clientSession,
                Movie = sessionMovie.Movie,
            };

            _db.Votes.Add(vote);
            await _db.SaveChangesAsync();


            _logger.LogInformation("участник {participantNumber} голосует за фильм {movieId} в сессии {sessionId}", participantNumber, movieId, sessionId);
            return ErrorFactory.Ok("Film liked successfully");
        }

        public async Task<BaseResponseDto> DislikeFilmsAsync(string clientId, int? movieId)
        {
            if (string.IsNullOrWhiteSpace(clientId) || !movieId.HasValue)
            {
                return ErrorFactory.Fail(ErrorType.BadRequest, "Client ID and movie ID cannot be empty");
            }

            var clientParticipant = await _db.SessionParticipants
                .FirstOrDefaultAsync(p => p.ClientId == clientId);
            if (clientParticipant == null)
            {
                return ErrorFactory.Fail(ErrorType.Conflict, "You are not a participant of any session");
            }

            var sessionId = clientParticipant.SessionId;
            var clientSession = await _db.Sessions
                .FirstOrDefaultAsync(s => s.Id == sessionId);
            if (clientSession == null)
            {
                return ErrorFactory.Fail(ErrorType.NotFound, "Session not found");
            }
            var sessionMovie = await _db.SessionMovies
                .FirstOrDefaultAsync(sm => sm.SessionId == sessionId && sm.MovieId == movieId.Value);
            if (sessionMovie == null)
            {
                return ErrorFactory.Fail(ErrorType.NotFound, "Movie not found in this session");
            }
            var participantNumber = clientParticipant.ParticipantNumber;
            if (participantNumber != 1 && participantNumber != 2)
            {
                return ErrorFactory.Fail(ErrorType.Conflict, "Invalid participant number");
            }

            var existingVote = await _db.Votes
            .AnyAsync(v => v.ParticipantNumber == participantNumber && v.SessionId == sessionId && v.MovieId == movieId);
            if (existingVote)
            {
                return ErrorFactory.Fail(ErrorType.Conflict, "You have already voted for this session");
            }

            var vote = new Vote
            {
                IsLiked = false,
                SessionId = sessionId,
                ParticipantNumber = participantNumber,
                MovieId = movieId.Value,
                Session = clientSession,
                Movie = sessionMovie.Movie,
            };

            _db.Votes.Add(vote);
            await _db.SaveChangesAsync();

            return ErrorFactory.Ok("Film disliked successfully");
        }


        public async Task<BaseResponseDto<List<MovieDto>>> GetMatchedInSessionMovieAsync(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return ErrorFactory.Fail<List<MovieDto>>(ErrorType.BadRequest, "Client ID cannot be empty");
            }
            var clientParticipant = await _db.SessionParticipants
                .FirstOrDefaultAsync(p => p.ClientId == clientId);
            if (clientParticipant == null)
            {
                return ErrorFactory.Fail<List<MovieDto>>(ErrorType.Conflict, "You are not a participant of any session");
            }
            var sessionId = clientParticipant.SessionId;
            var clientSession = await _db.Sessions
                .FirstOrDefaultAsync(s => s.Id == sessionId);
            if (clientSession == null)
            {
                return ErrorFactory.Fail<List<MovieDto>>(ErrorType.NotFound, "Session not found");
            }

            var matchedMovieIds = await _db.Votes
                .Where(v => v.SessionId == sessionId && v.IsLiked)
                .GroupBy(v => v.MovieId)
                .Where(g => g.Select(v => v.ParticipantNumber).Distinct().Count() == 2)
                .Select(g => g.Key)
                .ToListAsync();

            if (matchedMovieIds == null || matchedMovieIds.Count == 0)
            {
                return ErrorFactory.Fail<List<MovieDto>>(ErrorType.NoContent, "No matched movies found for this session");
            }

            List<MovieDto> matchedMovies = [];

            foreach (var movieId in matchedMovieIds)
            {
                var movie = await _db.Movies.FirstOrDefaultAsync(m => m.Id == movieId);
                if (movie != null)
                {
                    matchedMovies.Add(new MovieDto
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
            if (matchedMovies == null || matchedMovies.Count == 0)
            {
                return ErrorFactory.Fail<List<MovieDto>>(ErrorType.NoContent, "No matched movies found for this session");
            }


            return ErrorFactory.Ok(matchedMovies, "Matched movies retrieved successfully");
        }

        public async Task<BaseResponseDto> ClearSessionVotesAsync(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return ErrorFactory.Fail(ErrorType.BadRequest, "Client ID cannot be empty");
            }
            var sessionCreator = await _db.Sessions
                .FirstOrDefaultAsync(p => p.CreatorClientId == clientId);
            if (sessionCreator == null)
            {
                return ErrorFactory.Fail(ErrorType.Conflict, "You are not a creator of any session");
            }
            var sessionId = sessionCreator.Id;
            var votesToRemove = await _db.Votes.Where(v => v.SessionId == sessionId).ToListAsync();
            if (votesToRemove == null || votesToRemove.Count == 0)
            {
                return ErrorFactory.Fail(ErrorType.NoContent, "No votes to clear for this session");
            }
            _db.Votes.RemoveRange(votesToRemove);
            await _db.SaveChangesAsync();
            return ErrorFactory.Ok("Session votes cleared successfully");
        }

        public async Task<BaseResponseDto<MovieDto>> GetRandomMatchedFilmAsync(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                return ErrorFactory.Fail<MovieDto>(ErrorType.BadRequest, "Client ID cannot be empty");
            }
            var clientParticipant = await _db.SessionParticipants
                .FirstOrDefaultAsync(p => p.ClientId == clientId);
            if (clientParticipant == null)
            {
                return ErrorFactory.Fail<MovieDto>(ErrorType.Conflict, "You are not a participant of any session");
            }
            var sessionId = clientParticipant.SessionId;
            var matchedMovieIds = await _db.Votes
                .Where(v => v.SessionId == sessionId && v.IsLiked)
                .GroupBy(v => v.MovieId)
                .Where(g => g.Select(v => v.ParticipantNumber).Distinct().Count() == 2)
                .Select(g => g.Key)
                .ToListAsync();
            if (matchedMovieIds == null || matchedMovieIds.Count == 0)
            {
                return ErrorFactory.Fail<MovieDto>(ErrorType.NoContent, "No matched movies found for this session");
            }
            var random = new Random();
            var randomMovieId = matchedMovieIds[random.Next(matchedMovieIds.Count)];
            var movie = await _db.Movies.FirstOrDefaultAsync(m => m.Id == randomMovieId);
            if (movie == null)
            {
                return ErrorFactory.Fail<MovieDto>(ErrorType.NotFound, "Matched movie not found");
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
                Genres = movie.Genres,
            };

            return ErrorFactory.Ok(response, "Random matched movie retrieved successfully");
        }



        //private methods
        private async Task<string> GenerateCode(int lenght = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var random = new Random();

            while (true)
            {
                var code = new string(Enumerable.Repeat(chars, lenght)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                var exists = await _db.Sessions.AnyAsync(s => s.Code == code);
                if (!exists)
                {
                    return code;
                }
            }
        }

    }
}
