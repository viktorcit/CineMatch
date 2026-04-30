using CineMatch.Data.DTO;
using CineMatch.Data.DTO.MoviesDto;
using CineMatch.Enums;

namespace CineMatch.Services.Interfaces
{
    public interface IMovieService
    {
        Task<BaseResponseWithDataDto<MovieDto>> GetMovieAsync(string input, ContentType inputContentType, int? inputYear);
    }
}
