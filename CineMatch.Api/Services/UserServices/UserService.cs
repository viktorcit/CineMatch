using CineMatch.Api.Data;
using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Data.DTO.ResponsesDto.User;
using CineMatch.Api.Helpers;
using CineMatch.Api.Services.Interfaces.IUserServices;

namespace CineMatch.Api.Services.UserServices
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }


        public async Task<BaseResponseDto<UserResponseDto>> CreateUser(string? clientId)
        {
            return ErrorFactory.Ok<UserResponseDto>(null, "");
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
                //var exists = await _db.Users.AnyAsync(s => s.PublicId == Id);
                //if (!exists)
                //{
                //    return Id;
                //}
            }
        }

        //private static methods
        private static string GenerateSecret(int lenght = 10)
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
