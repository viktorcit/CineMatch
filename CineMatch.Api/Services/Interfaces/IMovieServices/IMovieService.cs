using CineMatch.Api.Data.Contracts;
using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Enums;

namespace CineMatch.Api.Services.Interfaces.IMovieServices
{
    public interface IMovieService
    {
        Task<List<MovieInfo>> GetAllMoviesAsync(CancellationToken cancellationToken);
        Task<BaseResponseDto<MovieInfo>> GetMovieByIdAsync(int id);
        Task<BaseResponseDto> SaveMovieAsync(int tmdbId, ContentType type, string userId, CancellationToken cancellationToken);
        Task<BaseResponseDto> DeleteMovieAsync(int id);
    }
}
