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
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }


        public async Task<BaseResponseDto> GetUserInfo(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ResponseFactory.Fail(ErrorType.NotFound);
            }
            var userRoles = await _userManager.GetRolesAsync(user);

            var sessionParticipant = await _db.SessionParticipants.FirstOrDefaultAsync(sp => sp.UserId == user.Id);
            if (sessionParticipant != null)
            {
                var session = await _db.Sessions.FindAsync(sessionParticipant.SessionId)
                    ?? throw new InvalidDataException("Session not found for the participant.");

                var sessionMoviesIds = await _db.SessionMovies
                    .Where(sm => sm.SessionId == session.Id)
                    .Select(sm => sm.MovieId)
                    .ToListAsync();

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

        public async Task<BaseResponseDto> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ResponseFactory.Fail(ErrorType.NotFound);
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, RolesName.Admin);
            if (isAdmin)
            {
                return ResponseFactory.Fail(ErrorType.Forbidden);
            }

            await _userManager.DeleteAsync(user);
            return ResponseFactory.Ok("Delete successfully.");
        }


        //private methods

        //private static methods
    }
}
