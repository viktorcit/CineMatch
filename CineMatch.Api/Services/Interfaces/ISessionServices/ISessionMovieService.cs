using CineMatch.Api.Data.Contracts;
using CineMatch.Api.Data.DTO.ResponsesDto;

namespace CineMatch.Api.Services.Interfaces.ISessionServices
{
    public interface ISessionMovieService
    {
        Task<BaseResponseDto<List<MovieInfo>>> GetFilmsOfSessionAsync(string clientId, string userId, CancellationToken cancellationToken);
        Task<BaseResponseDto<List<MovieInfo>>> GetMatchedInSessionMovieAsync(string clientId, string userId, CancellationToken cancellationToken);
        Task<BaseResponseDto<MovieInfo>> GetRandomMatchedFilmAsync(string clientId, string userId, CancellationToken cancellationToken);
        Task<BaseResponseDto> DeleteMovieFromSessionAsync(int movieId, string userId);
    }
}
