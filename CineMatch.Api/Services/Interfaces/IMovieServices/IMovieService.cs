using CineMatch.Api.Data.Contracts;
using CineMatch.Api.Data.DTO.ResponsesDto;

namespace CineMatch.Api.Services.Interfaces.IMovieServices
{
    public interface IMovieService
    {
        Task<BaseResponseDto> SaveMovieAsync(MovieInfo dto, string clientId);
        Task<List<MovieInfo>> GetAllMoviesAsync();
        Task<BaseResponseDto> DeleteMovieAsync(int id);
        Task<BaseResponseDto<MovieInfo>> GetMovieByIdAsync(int id);
    }
}
