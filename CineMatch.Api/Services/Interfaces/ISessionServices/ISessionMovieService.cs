using CineMatch.Api.Data.Contracts;
using CineMatch.Api.Data.DTO.ResponsesDto;

namespace CineMatch.Api.Services.Interfaces.ISessionServices
{
    public interface ISessionMovieService
    {
        Task<BaseResponseDto<List<MovieInfo>>> GetFilmsOfSessionAsync(string clientId, string userId);
        Task<BaseResponseDto<List<MovieInfo>>> GetMatchedInSessionMovieAsync(string clientId, string userId);
        Task<BaseResponseDto<MovieInfo>> GetRandomMatchedFilmAsync(string clientId, string userId);
    }
}
