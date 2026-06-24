using CineMatch.Api.Data;
using CineMatch.Api.Data.DTO;
using CineMatch.Api.Data.DTO.UserDto;
using CineMatch.Api.Enums;
using CineMatch.Api.Model;
using CineMatch.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineMatch.Api.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly AppDbContext _db;

        public UserService(ILogger<UserService> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }


        public async Task<BaseResponseWithDataDto<UserDto>> CreateUser(string? clientId)
        {
            if (!string.IsNullOrEmpty(clientId))
            {
                return new BaseResponseWithDataDto<UserDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "You already have account",
                    Data = null
                };
            }

            var PublicId = await GenerateId();
            var Secret = GenerateSecret();

            var user = new User
            {
                PublicId = PublicId,
                Secret = Secret,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var response = new UserDto
            {
                Id = user.Id,
                PublicId = user.PublicId,
                Secret = user.Secret,
                CreatedAt = user.CreatedAt
            };

            return new BaseResponseWithDataDto<UserDto>
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "User created successfully",
                Data = response
            };
        }



        //private methods
        private async Task<string> GenerateId(int lenght = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var random = new Random();

            while (true)
            {
                var Id = new string(Enumerable.Repeat(chars, lenght)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                var exists = await _db.Users.AnyAsync(s => s.PublicId == Id);
                if (!exists)
                {
                    return Id;
                }
            }
        }

        private string GenerateSecret(int lenght = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var random = new Random();

            while (true)
            {
                var Secret = new string(Enumerable.Repeat(chars, lenght)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                return Secret;
            }
        }
    }
}
