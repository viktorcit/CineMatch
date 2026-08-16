
namespace CineMatch.Api.Services.Interfaces.IJwtServices
{
    public interface IRefreshTokenService
    {
        Task<string?> RefreshToken(string oldRefreshToken);
        Task<string> CreateRefreshToken(string userId);
    }
}
