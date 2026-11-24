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


        public async Task<BaseResponseDto> LikeFilmsAsync(string userId, int movieId)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "LikeFilmsAsync", ["UserId"] = userId });
            var sessionId = await PrepareVoteAsync(userId, movieId);
            if (sessionId == null)
            {
                return ResponseFactory.Fail(ErrorType.Conflict, "You cannot vote.");
            }

            var vote = new Vote
            {
                IsLiked = true,
                SessionId = sessionId.Value,
                MovieId = movieId,
                ParticipantId = userId
            };

            _db.Votes.Add(vote);
            await _db.SaveChangesAsync();

            _logger.LogDebug("Like recorded for movie {MovieId} by user {UserId} in session {SessionId}", movieId, userId, sessionId);
            return ResponseFactory.Ok("Film liked successfully");
        }

        public async Task<BaseResponseDto> DislikeFilmsAsync(string userId, int movieId)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "DislikeFilmsAsync", ["UserId"] = userId });
            var sessionId = await PrepareVoteAsync(userId, movieId);
            if (sessionId == null)
            {
                return ResponseFactory.Fail(ErrorType.Conflict, "You cannot vote.");
            }

            var vote = new Vote
            {
                IsLiked = false,
                SessionId = sessionId.Value,
                MovieId = movieId,
                ParticipantId = userId
            };

            _db.Votes.Add(vote);
            await _db.SaveChangesAsync();

            _logger.LogDebug("Dislike recorded for movie {MovieId} by user {UserId} in session {SessionId}", movieId, userId, sessionId);
            return ResponseFactory.Ok("Film disliked successfully");
        }

        public async Task<BaseResponseDto> ClearSessionVotesAsync(string userId)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "ClearSessionVotesAsync", ["UserId"] = userId });
            var sessionCreator = await _db.Sessions
                .FirstOrDefaultAsync(p => p.CreatorUserId == userId);
            if (sessionCreator == null)
            {
                return ResponseFactory.Fail(ErrorType.Conflict, "You are not a creator of any session");
            }

            var votesToRemove = await _db.Votes.Where(v => v.SessionId == sessionCreator.Id).ToListAsync();
            if (votesToRemove == null || votesToRemove.Count == 0)
            {
                return ResponseFactory.Fail(ErrorType.NoContent, "No votes to clear for this session");
            }
            _db.Votes.RemoveRange(votesToRemove);
            await _db.SaveChangesAsync();
            _logger.LogInformation("User {UserId} cleared {VoteCount} votes in session {SessionId}", userId, votesToRemove.Count, sessionCreator.Id);
            return ResponseFactory.Ok("Session votes cleared successfully");
        }


        //private methods
        private async Task<int?> PrepareVoteAsync(string userId, int movieId)
        {
            var clientParticipant = await _db.SessionParticipants
                .FirstOrDefaultAsync(p => p.UserId == userId);
            if (clientParticipant == null)
            {
                _logger.LogDebug("Vote rejected: user {UserId} is not in a session", userId);
                return null;
            }

            var sessionId = clientParticipant.SessionId;
            var sessionMovie = await _db.SessionMovies
                .FirstOrDefaultAsync(sm => sm.SessionId == sessionId && sm.MovieId == movieId);
            if (sessionMovie == null)
            {
                _logger.LogDebug("Vote rejected: movie {MovieId} is not in session {SessionId} for user {UserId}", movieId, sessionId, userId);
                return null;
            }

            var existingVote = await _db.Votes
                .AnyAsync(v => v.ParticipantId == userId && v.SessionId == sessionId && v.MovieId == movieId);
            if (existingVote)
            {
                _logger.LogDebug("Duplicate vote rejected for movie {MovieId} by user {UserId} in session {SessionId}", movieId, userId, sessionId);
                return null;
            }

            return sessionId;
        }
    }
}
