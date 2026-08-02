using CineMatch.Api.Data.Contracts;
using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Enums;

namespace CineMatch.Api.Services.Interfaces.IMovieServices
{
    public interface IMovieSearchService
    {
        Task<BaseResponseDto<MovieInfo>> GetMovieByUrlAsync(string inputUrl);
        Task<BaseResponseDto<List<MovieInfo>>> GetMovieBySearchAsync(string mainInput, ContentType inputContentType, int? inputYear);
    }
}
