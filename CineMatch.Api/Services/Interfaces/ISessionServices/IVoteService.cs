using CineMatch.Api.Data.DTO.ResponsesDto;

namespace CineMatch.Api.Services.Interfaces.ISessionServices
{
    public interface IVoteService
    {
        Task<BaseResponseDto> LikeFilmsAsync(string userId, int movieId);
        Task<BaseResponseDto> DislikeFilmsAsync(string userId, int movieId);
        Task<BaseResponseDto> ClearSessionVotesAsync(string userId);
    }
}
