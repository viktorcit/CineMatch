using CineMatch.Data.DTO;
using CineMatch.Data.DTO.MoviesDto;

namespace CineMatch.Services.Interfaces
{
    public interface IMovieService
    {
        Task<BaseResponseWithDataDto<MovieDto>> SaveMovieAsync(MovieDto dto);
        Task<List<MovieDto>> GetAllMoviesAsync();
        Task<BaseResponseDto> DeleteMovieAsync(int id);
        Task<BaseResponseWithDataDto<MovieDto>> GetMovieByIdAsync(int id);
    }
}
