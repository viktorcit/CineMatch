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
            _logger.LogInformation("создание сессии");
            var existingParticipant = await _db.SessionParticipants
                .AnyAsync(p => p.UserId == userId);
            if (existingParticipant)
            {
                return ErrorFactory.Fail<SessionResponseDto>(ErrorType.Conflict, "You are already a participant of another session. Leave for creation");
            }

            var session = await CreateSessionEntityAsync(userId);
            int participantNumber = 1;
            var participant = CreateSessionParticipantEntity(userId, session, participantNumber);

            _db.SessionParticipants.Add(participant);
            _db.Sessions.Add(session);
            await _db.SaveChangesAsync();

            var response = CreateSessionResponseDto(session);

            return ErrorFactory.Ok(response, "Session created successfully");
        }


        public async Task<BaseResponseDto> JoinToSessionAsync(string code, string userId)
        {
            var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Code == code);
            if (session == null)
            {
                return ErrorFactory.Fail(ErrorType.NotFound, "Session not found");
            }

            var existingParticipant = await _db.SessionParticipants.AnyAsync(p => p.UserId == userId);
            if (existingParticipant)
            {
                return ErrorFactory.Fail(ErrorType.Conflict, "You are already a participant of another session.");
            }

            var sessionParticipants = await _db.SessionParticipants
                .Where(p => p.SessionId == session.Id).ToListAsync();
            if (sessionParticipants.Count >= 2)
            {
                return ErrorFactory.Fail(ErrorType.Conflict, "Session is full");
            }

            var participantNumber = sessionParticipants.Count + 1;
            var newParticipant = CreateSessionParticipantEntity(userId, session, participantNumber);

            _db.SessionParticipants.Add(newParticipant);
            await _db.SaveChangesAsync();

            return ErrorFactory.Ok("Joined session successfully");
        }


        public async Task<BaseResponseDto> LeaveSessionAsync(string userId)
        {
            var sessionParticipant = await _db.SessionParticipants
                .FirstOrDefaultAsync(p => p.UserId == userId);
            if (sessionParticipant == null)
            {
                return ErrorFactory.Fail(ErrorType.Conflict, "You are not a participant in any session");
            }
            var sessionCreatorExist = await _db.Sessions
                .AnyAsync(s => s.Id == sessionParticipant.SessionId && s.CreatorUserId == userId);
            if (sessionCreatorExist)
            {
                return ErrorFactory.Fail(ErrorType.Conflict, "You are a creator of this session. End session for leaving");
            }

            _db.SessionParticipants.Remove(sessionParticipant);
            await _db.SaveChangesAsync();

            return ErrorFactory.Ok("Left session successfully");
        }

        public async Task<BaseResponseDto> EndSessionAsync(string userId)
        {
            var sessionCreator = await _db.Sessions
                .FirstOrDefaultAsync(p => p.CreatorUserId == userId);
            if (sessionCreator == null)
            {
                return ErrorFactory.Fail(ErrorType.Conflict, "You don't have an active session that you created to end it.");
            }

            _db.Sessions.Remove(sessionCreator);
            await _db.SaveChangesAsync();
            return ErrorFactory.Ok("Session ended successfully");
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
