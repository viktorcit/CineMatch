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
            var sessionId = await PrepareVote(userId, movieId);
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

            _logger.LogInformation("участник {participantId} голосует за фильм {movieId} в сессии {sessionId}", userId, movieId, sessionId);
            return ResponseFactory.Ok("Film liked successfully");
        }

        public async Task<BaseResponseDto> DislikeFilmsAsync(string userId, int movieId)
        {
            var sessionId = await PrepareVote(userId, movieId);
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

            return ResponseFactory.Ok("Film disliked successfully");
        }

        public async Task<BaseResponseDto> ClearSessionVotesAsync(string userId)
        {
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
            return ResponseFactory.Ok("Session votes cleared successfully");
        }


        //private methods
        private async Task<int?> PrepareVote(string userId, int movieId)
        {
            var clientParticipant = await _db.SessionParticipants
                .FirstOrDefaultAsync(p => p.UserId == userId);
            if (clientParticipant == null)
            {
                return null;
            }

            var sessionId = clientParticipant.SessionId;
            var sessionMovie = await _db.SessionMovies
                .FirstOrDefaultAsync(sm => sm.SessionId == sessionId && sm.MovieId == movieId);
            if (sessionMovie == null)
            {
                return null;
            }

            var existingVote = await _db.Votes
                .AnyAsync(v => v.ParticipantId == userId && v.SessionId == sessionId && v.MovieId == movieId);
            if (existingVote)
            {
                return null;
            }

            return sessionId;
        }
    }
}
