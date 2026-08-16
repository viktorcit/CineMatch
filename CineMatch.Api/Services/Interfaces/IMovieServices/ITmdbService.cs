using CineMatch.Api.Data.Contracts;
using CineMatch.Api.Enums;
using System.Text.Json;

namespace CineMatch.Api.Services.Interfaces.IMovieServices
{
    public interface ITmdbService
    {
        Task<MovieInfo?> GetMovieDetailsAsync(int movieId, ContentType type);
        Task<List<MovieInfo>?> GetMovieDetailsFromSearchAsync(string title, ContentType type, int? year);
    }
}
