
namespace CineMatch.Api.Services.Interfaces.IJwtServices
{
    public interface IRefreshTokenService
    {
        Task<string?> RefreshTokenAsync(string oldRefreshToken);
        Task<string> CreateRefreshTokenAsync(string userId);
    }
}
