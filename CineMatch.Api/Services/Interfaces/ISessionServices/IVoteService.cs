using CineMatch.Api.Data.DTO.ResponsesDto;

namespace CineMatch.Api.Services.Interfaces.ISessionServices
{
    public interface IVoteService
    {
        Task<BaseResponseDto> LikeFilmsAsync(string clientId, int? movieId);
        Task<BaseResponseDto> DislikeFilmsAsync(string clientId, int? movieId);
        Task<BaseResponseDto> ClearSessionVotesAsync(string clientId);
    }
}
