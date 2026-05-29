using CineMatch.Api.Data.DTO;
using CineMatch.Api.Data.DTO.MoviesDto;
using CineMatch.Api.Enums;

namespace CineMatch.Api.Services.Interfaces
{
    public interface IMovieSearchService
    {
        Task<BaseResponseWithDataDto<MovieDto>> GetMovieByUrlAsync(string inputUrl);
        Task<BaseResponseWithDataDto<List<MovieDto>>> GetMovieBySearchAsync(string mainInput, ContentType inputContentType, int? inputYear);
    }
}
