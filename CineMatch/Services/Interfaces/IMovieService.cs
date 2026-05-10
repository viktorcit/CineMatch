using CineMatch.Data.DTO;
using CineMatch.Data.DTO.MoviesDto;
using CineMatch.Data.DTO.MoviesDTO;

namespace CineMatch.Services.Interfaces
{
    public interface IMovieService
    {
        Task<BaseResponseWithDataDto<SaveMovieDto>> SaveMovieAsync(MovieDto dto, string clientId);
        Task<List<MovieDto>> GetAllMoviesAsync();
        Task<BaseResponseDto> DeleteMovieAsync(int id);
        Task<BaseResponseWithDataDto<MovieDto>> GetMovieByIdAsync(int id);
    }
}
