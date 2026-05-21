using CineMatch.Data.DTO;
using CineMatch.Data.DTO.MoviesDto;
using CineMatch.Data.DTO.SessionDto;

namespace CineMatch.Services.Interfaces
{
    public interface ISessionService
    {
        Task<BaseResponseWithDataDto<SessionDto>> CreateSessionAsync(string clientId);
        Task<BaseResponseDto> JoinToSessionAsync(string code, string clientId);
        Task<BaseResponseWithDataDto<List<MovieDto>>> GetFilmsOfSessionAsync(string clientId);
        Task<BaseResponseDto> LeaveSessionAsync(string clientId);
        Task<BaseResponseDto> EndSessionAsync(string clientId);
        Task<BaseResponseDto> LikeFilmsAsync(string clientId, int? movieId);
        Task<BaseResponseDto> DislikeFilmsAsync(string clientId, int? movieId);
        Task<BaseResponseWithDataDto<List<MovieDto>>> GetMatchedInSessionMovieAsync(string clientId);
        Task<BaseResponseDto> ClearSessionVotesAsync(string clientId);
        Task<BaseResponseWithDataDto<MovieDto>> GetRandomMatchedFilmAsync(string clientId);

    }
}
