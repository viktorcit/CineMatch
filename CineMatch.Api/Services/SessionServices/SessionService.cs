using CineMatch.Api.Data;
using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Data.DTO.ResponsesDto.Session;
using CineMatch.Api.Entity;
using CineMatch.Api.Enums;
using CineMatch.Api.Helpers;
using CineMatch.Api.Services.Interfaces.ISessionServices;
using Microsoft.EntityFrameworkCore;

namespace CineMatch.Api.Services.SessionServices
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



        public async Task<BaseResponseDto<SessionResponseDto>> CreateSessionAsync(string userId)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "CreateSessionAsync", ["UserId"] = userId });
            var existingParticipant = await _db.SessionParticipants
                .AnyAsync(p => p.UserId == userId);
            if (existingParticipant)
            {
                _logger.LogDebug("Operation rejected: {Reason}", "You are already a participant of another session. Leave for creation");
                return ResponseFactory.Fail<SessionResponseDto>(ErrorType.Conflict, "You are already a participant of another session. Leave for creation");
            }

            var session = await CreateSessionEntityAsync(userId);
            int participantNumber = 1;
            var participant = CreateSessionParticipantEntity(userId, session, participantNumber);

            _db.SessionParticipants.Add(participant);
            _db.Sessions.Add(session);
            await _db.SaveChangesAsync();

            var response = CreateSessionResponseDto(session);

            _logger.LogInformation("Session {SessionId} created by user {UserId}", session.Id, userId);
            return ResponseFactory.Ok(response);
        }


        public async Task<BaseResponseDto> JoinToSessionAsync(string code, string userId)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "JoinToSessionAsync", ["UserId"] = userId });
            var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Code == code);
            if (session == null)
            {
                _logger.LogDebug("Operation rejected: {Reason}", "Session not found");
                return ResponseFactory.Fail(ErrorType.NotFound, "Session not found");
            }

            var existingParticipant = await _db.SessionParticipants.AnyAsync(p => p.UserId == userId);
            if (existingParticipant)
            {
                _logger.LogDebug("Operation rejected: {Reason}", "You are already a participant of another session.");
                return ResponseFactory.Fail(ErrorType.Conflict, "You are already a participant of another session.");
            }

            var sessionParticipants = await _db.SessionParticipants
                .Where(p => p.SessionId == session.Id).ToListAsync();
            if (sessionParticipants.Count >= 2)
            {
                _logger.LogDebug("Operation rejected: {Reason}", "Session is full");
                return ResponseFactory.Fail(ErrorType.Conflict, "Session is full");
            }

            var participantNumber = sessionParticipants.Count + 1;
            var newParticipant = CreateSessionParticipantEntity(userId, session, participantNumber);

            _db.SessionParticipants.Add(newParticipant);
            await _db.SaveChangesAsync();

            _logger.LogInformation("User {UserId} joined session {SessionId}", userId, session.Id);
            return ResponseFactory.Ok("Joined session successfully");
        }


        public async Task<BaseResponseDto> LeaveSessionAsync(string userId)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "LeaveSessionAsync", ["UserId"] = userId });
            var sessionParticipant = await _db.SessionParticipants
                .FirstOrDefaultAsync(p => p.UserId == userId);
            if (sessionParticipant == null)
            {
                _logger.LogDebug("Operation rejected: {Reason}", "You are not a participant in any session");
                return ResponseFactory.Fail(ErrorType.Conflict, "You are not a participant in any session");
            }
            var sessionCreatorExist = await _db.Sessions
                .AnyAsync(s => s.Id == sessionParticipant.SessionId && s.CreatorUserId == userId);
            if (sessionCreatorExist)
            {
                _logger.LogDebug("Operation rejected: {Reason}", "You are a creator of this session. End session for leaving");
                return ResponseFactory.Fail(ErrorType.Conflict, "You are a creator of this session. End session for leaving");
            }

            _db.SessionParticipants.Remove(sessionParticipant);
            await _db.SaveChangesAsync();

            _logger.LogInformation("User {UserId} left session {SessionId}", userId, sessionParticipant.SessionId);
            return ResponseFactory.Ok("Left session successfully");
        }

        public async Task<BaseResponseDto> EndSessionAsync(string userId)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "EndSessionAsync", ["UserId"] = userId });
            var sessionCreator = await _db.Sessions
                .FirstOrDefaultAsync(p => p.CreatorUserId == userId);
            if (sessionCreator == null)
            {
                _logger.LogDebug("Operation rejected: {Reason}", "You don't have an active session that you created to end it.");
                return ResponseFactory.Fail(ErrorType.Conflict, "You don't have an active session that you created to end it.");
            }

            _db.Sessions.Remove(sessionCreator);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Session {SessionId} ended by creator {UserId}", sessionCreator.Id, userId);
            return ResponseFactory.Ok("Session ended successfully");
        }



        //private methods
        private async Task<string> GenerateCodeAsync(int lenght = 6)
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

        private async Task<Session> CreateSessionEntityAsync(string clientId)
        {
            var session = new Session
            {
                Code = await GenerateCodeAsync(),
                CreatedAt = DateTime.UtcNow,
                CreatorUserId = clientId,
            };
            return session;
        }

        //private static methods
        private static SessionParticipant CreateSessionParticipantEntity(string clientId, Session session, int participantNumber)
        {
            var participant = new SessionParticipant
            {
                UserId = clientId,
                SessionId = session.Id,
                ParticipantNumber = participantNumber,
            };
            return participant;
        }

        private static SessionResponseDto CreateSessionResponseDto(Session session)
        {
            var response = new SessionResponseDto
            {
                Code = session.Code,
                CreatorUserId = session.CreatorUserId
            };
            return response;
        }
    }
}
