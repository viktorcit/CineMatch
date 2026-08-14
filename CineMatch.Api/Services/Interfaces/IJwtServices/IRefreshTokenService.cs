
namespace CineMatch.Api.Services.Interfaces.IJwtServices
{
    public interface IRefreshTokenService
    {
        Task<string?> RefreshToken(string oldRefreshToken, string userId);
        Task<string> CreateRefreshToken(string userId);
    }
}
