using CineMatch.Api.Data.DTO;
using CineMatch.Api.Model;

namespace CineMatch.Api.Services.Interfaces
{
    public interface IJwtTokenService
    {
        Task<BaseResponseWithDataDto<string>> GenerateTokenAsync(ApplicationUser user);
    }
}
