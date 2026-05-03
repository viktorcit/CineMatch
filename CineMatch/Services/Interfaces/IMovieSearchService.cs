using CineMatch.Data.DTO;
using CineMatch.Data.DTO.MoviesDto;
using CineMatch.Enums;

namespace CineMatch.Services.Interfaces
{
    public interface IMovieSearchService
    {
        Task<BaseResponseWithDataDto<MovieDto>> GetMovieByUrlAsync(string inputUrl);
        Task<BaseResponseWithDataDto<List<MovieDto>>> GetMovieBySearchAsync(string mainInput, ContentType inputContentType, int? inputYear);
    }
}
