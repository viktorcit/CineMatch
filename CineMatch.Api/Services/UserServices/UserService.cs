using CineMatch.Api.Data;
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



        //private methods

        //private static methods
    }
}
