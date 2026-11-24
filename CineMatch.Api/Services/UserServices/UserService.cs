using CineMatch.Api.Data;
using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Data.DTO.ResponsesDto.User;
using CineMatch.Api.Entity;
using CineMatch.Api.Enums;
using CineMatch.Api.Helpers;
using CineMatch.Api.Services.Interfaces.IUserServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CineMatch.Api.Services.UserServices
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(AppDbContext db, UserManager<ApplicationUser> userManager, ILogger<UserService> logger)
        {
            _logger = logger;
            _db = db;
            _userManager = userManager;
        }


        public async Task<BaseResponseDto<AdminUserInfoResponseDto>> GetUserInfoAsync(string userId, CancellationToken cancellationToken)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "GetUserInfoAsync", ["UserId"] = userId });
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ResponseFactory.Fail<AdminUserInfoResponseDto>(ErrorType.NotFound);
            }
            var userRoles = await _userManager.GetRolesAsync(user);

            var sessionParticipant = await _db.SessionParticipants.FirstOrDefaultAsync(sp => sp.UserId == user.Id, cancellationToken);
            if (sessionParticipant != null)
            {
                var session = await _db.Sessions.FindAsync([sessionParticipant.SessionId], cancellationToken)
                    ?? throw new InvalidDataException("Session not found for the participant.");

                var sessionMoviesIds = await _db.SessionMovies
                    .Where(sm => sm.SessionId == session.Id)
                    .Select(sm => sm.MovieId)
                    .ToListAsync(cancellationToken);

                var response = new AdminUserInfoResponseDto
                {
                    UserId = user.Id,
                    CreatedAt = user.CreatedAt,
                    UserName = user.UserName,
                    UserRoles = userRoles.ToList(),
                    UserParticipantNumber = sessionParticipant.ParticipantNumber,
                    SessionCode = session.Code,
                    SessionId = session.Id,
                    SessionMovieId = sessionMoviesIds
                };
                return ResponseFactory.Ok(response);
            }

            var response2 = new AdminUserInfoResponseDto
            {
                UserId = user.Id,
                CreatedAt = user.CreatedAt,
                UserName = user.UserName,
                UserRoles = userRoles.ToList()
            };
            return ResponseFactory.Ok(response2);
        }

        public async Task<BaseResponseDto> DeleteUserAsync(string userId)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "DeleteUserAsync", ["UserId"] = userId });
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ResponseFactory.Fail(ErrorType.NotFound);
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, RolesName.Admin);
            if (isAdmin)
            {
                _logger.LogWarning("Administrative deletion rejected: target user {UserId} has the administrator role", userId);
                return ResponseFactory.Fail(ErrorType.Forbidden);
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var error = result.Errors.FirstOrDefault();
                var handlerResponse = IdentityErrorHandler.ErrorHandler(error);
                _logger.Log(handlerResponse.ErrorType == ErrorType.ServerError ? LogLevel.Error : LogLevel.Warning, "Administrative user deletion failed for target {UserId}. Identity codes: {ErrorCodes}", userId, string.Join(", ", result.Errors.Select(e => e.Code)));
                return ResponseFactory.Fail(handlerResponse.ErrorType, handlerResponse.ResponseMessage);
            }
            _logger.LogInformation("User {UserId} deleted through the administrative operation", userId);
            return ResponseFactory.Ok("Delete successfully.");
        }


        //private methods

        //private static methods
    }
}
