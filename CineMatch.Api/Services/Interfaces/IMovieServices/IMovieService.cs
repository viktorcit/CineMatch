using CineMatch.Api.Data.Contracts;
using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Enums;

namespace CineMatch.Api.Services.Interfaces.IMovieServices
{
    public interface IMovieService
    {
        Task<BaseResponseDto> SaveMovieAsync(int tmdbId, ContentType type, string userId);
        Task<List<MovieInfo>> GetAllMoviesAsync();
        Task<BaseResponseDto> DeleteMovieAsync(int id);
        Task<BaseResponseDto<MovieInfo>> GetMovieByIdAsync(int id);
    }
}
