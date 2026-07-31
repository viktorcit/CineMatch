using CineMatch.Api.Data.DTO;
using CineMatch.Api.Data.DTO.MoviesDto;
using CineMatch.Api.Data.DTO.MoviesDTO;

namespace CineMatch.Api.Services.Interfaces
{
    public interface IMovieService
    {
        Task<BaseResponseDto> SaveMovieAsync(MovieDto dto, string clientId);
        Task<List<MovieDto>> GetAllMoviesAsync();
        Task<BaseResponseDto> DeleteMovieAsync(int id);
        Task<BaseResponseDto<MovieDto>> GetMovieByIdAsync(int id);
    }
}
