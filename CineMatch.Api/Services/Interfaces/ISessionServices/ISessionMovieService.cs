using CineMatch.Api.Data.Contracts;
using CineMatch.Api.Data.DTO.ResponsesDto;

namespace CineMatch.Api.Services.Interfaces.ISessionServices
{
    public interface ISessionMovieService
    {
        Task<BaseResponseDto<List<MovieInfo>>> GetFilmsOfSessionAsync(string clientId);
        Task<BaseResponseDto<List<MovieInfo>>> GetMatchedInSessionMovieAsync(string clientId);
        Task<BaseResponseDto<MovieInfo>> GetRandomMatchedFilmAsync(string clientId);
    }
}
