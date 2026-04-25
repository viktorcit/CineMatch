using CineMatch.Data.DTO;
using CineMatch.Data.DTO.MoviesDto;

namespace CineMatch.Services.Interfaces
{
    public interface IMovieService
    {
        Task<BaseResponseWithDataDto<MovieDto>> GetMovieAsync(string inputUrl);
    }
}
