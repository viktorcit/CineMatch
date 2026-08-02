using CineMatch.Api.Data;
using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Entity;
using CineMatch.Api.Enums;
using CineMatch.Api.Helpers;
using CineMatch.Api.Services.Interfaces.ISessionServices;
using Microsoft.EntityFrameworkCore;

namespace CineMatch.Api.Services.SessionServices
{
    public class VoteService : IVoteService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<VoteService> _logger;

        public VoteService(AppDbContext db, ILogger<VoteService> logger)
        {
            _db = db;
            _logger = logger;
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
    }
}
