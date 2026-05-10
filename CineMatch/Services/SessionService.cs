using CineMatch.Data;
using CineMatch.Data.DTO;
using CineMatch.Data.DTO.SessionDto;
using CineMatch.Enums;
using CineMatch.Model;
using CineMatch.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineMatch.Services
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



        public async Task<BaseResponseWithDataDto<SessionDto>> CreateSessionAsync(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return new BaseResponseWithDataDto<SessionDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Client ID cannot be empty",
                };
            }
            _logger.LogInformation("создание сессии");
            var existingSession = await _db.Sessions.AnyAsync(s => s.CreatorClientId == clientId && s.IsActive);
            if (existingSession)
            {
                return new BaseResponseWithDataDto<SessionDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "You already have an active session",
                };
            }

            var session = new Session
            {
                Code = await GenerateCode(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
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
                IsActive = session.IsActive,
                CreatorClientId = session.CreatorClientId,
                SessionMovies = new List<SessionMovie>(),
                Votes = new List<Vote>(),
            };


            return new BaseResponseWithDataDto<SessionDto>
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "Session created successfully",
                Data = response,
            };
        }


        public async Task<BaseResponseDto> JoinToSession(string code, string clientId)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Session code cannot be empty",
                };
            }
            var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Code == code);
            if (session == null)
            {
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.NotFound,
                    ResponseMessage = "Session not found",
                };
            }
            var alreadyJoined = await _db.SessionParticipants
                .FirstOrDefaultAsync(p => p.ClientId == clientId && p.SessionId == session.Id);
            if (alreadyJoined != null)
            {
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "You are already a participant of this session",
                };
            }
            var existingParticipant = await _db.SessionParticipants.AnyAsync(p => p.ClientId == clientId);
            if (existingParticipant)
            {
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "You are already a participant of another session",
                };
            }
            if (session.Participants.Count() >= 2)
            {
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Session is full",
                };
            }
            if (!session.IsActive)
            {
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Session is not active",
                };
            }
            var participantNumber = session.Participants.Count + 1;

            var newParticipant = new SessionParticipant
            {
                ClientId = clientId,
                SessionId = session.Id,
                Session = session,
                ParticipantNumber = participantNumber,
            };

            _db.SessionParticipants.Add(newParticipant);
            await _db.SaveChangesAsync();

            return new BaseResponseDto
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "Joined session successfully",
            };
        }


        public async Task<BaseResponseDto> GetFilmsOfSession()
        {

        }


        public async Task<BaseResponseDto> LeaveSession(string clientId)
        {

        }

        public async Task<BaseResponseDto> LikeFilms()
        {

        }


        public async Task<BaseResponseDto> DislikeFilms()
        {

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
