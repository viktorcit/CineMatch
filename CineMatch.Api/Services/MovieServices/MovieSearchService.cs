using CineMatch.Api.Data.Contracts;
using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Entity;
using CineMatch.Api.Enums;
using CineMatch.Api.Helpers;
using CineMatch.Api.Services.Interfaces.IMovieServices;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CineMatch.Api.Services.MovieServices
{
    public class MovieSearchService : IMovieSearchService
    {
        private readonly ILogger<MovieSearchService> _logger;
        private readonly ITmdbService _tmdbService;
        public MovieSearchService
            (ILogger<MovieSearchService> logger,
            ITmdbService tmdbService)
        {
            _logger = logger;
            _tmdbService = tmdbService;
        }



        public async Task<BaseResponseDto<MovieInfo>> GetMovieByUrlAsync(string inputUrl)
        {
            if (string.IsNullOrWhiteSpace(inputUrl))
            {
                _logger.LogInformation("поле ссылки пусто");
                return ErrorFactory.Fail<MovieInfo>(ErrorType.BadRequest, "Input cannot be null.");
            }
            if (!IsTmdbLink(inputUrl))
            {
                return ErrorFactory.Fail<MovieInfo>(ErrorType.BadRequest, ResponseMessages.InvalidTmdbUrl);
            }

            try
            {
                var movieId = ExtractIdFromLink(inputUrl);
                if (movieId == 0)
                {
                    _logger.LogInformation("неправильная ссылка");
                    return ErrorFactory.Fail<MovieInfo>(ErrorType.BadRequest, ResponseMessages.InvalidTmdbUrl);
                }

                var contentType = ContentTypeCheck(inputUrl);
                if (contentType == ContentType.Unknown)
                {
                    _logger.LogInformation("неправильная ссылка");
                    return ErrorFactory.Fail<MovieInfo>(ErrorType.BadRequest, ResponseMessages.InvalidTmdbUrl);
                }

                var movieDetails = await _tmdbService.GetMovieDetailsAsync(movieId, contentType);
                if (movieDetails == null)
                {
                    _logger.LogInformation("фильм не найден");
                    return ErrorFactory.Fail<MovieInfo>(ErrorType.NotFound, "Movie not found");
                }
                _logger.LogInformation("фильм найден");
                return ErrorFactory.Ok(movieDetails, "Movie details fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"An error occurred during get movie by id: {ex.Message}");
                return ErrorFactory.Fail<MovieInfo>(ErrorType.ServerError, ResponseMessages.ServerError);
            }
        }

        public async Task<BaseResponseDto<List<MovieInfo>>> GetMovieBySearchAsync(string mainInput, ContentType inputContentType, int? inputYear)
        {
            if (string.IsNullOrWhiteSpace(mainInput))
            {
                _logger.LogInformation("Поле ввода пустое");
                return ErrorFactory.Fail<List<MovieInfo>>(ErrorType.BadRequest, "Input cannot be null");
            }

            try
            {
                var movieDetails = await _tmdbService.GetMovieDetailsFromSearchAsync(mainInput, inputContentType, inputYear);
                if (movieDetails == null)
                {
                    _logger.LogInformation("фильм не найден");
                    return ErrorFactory.Fail<List<MovieInfo>>(ErrorType.BadRequest, "Movie not found.");
                }
                _logger.LogInformation("фильм найден");
                return ErrorFactory.Ok(movieDetails, "Movie details fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"An error occurred while searching for the movie: {ex.Message}");
                return ErrorFactory.Fail<List<MovieInfo>>(ErrorType.ServerError, ResponseMessages.ServerError);
            }
        }


        //private methods

        //static private methods

        private static int ExtractIdFromLink(string inputUrl)
        {
            var marker = "/movie/";
            var markerTv = "/tv/";
            if (inputUrl.Contains(markerTv))
            {
                marker = markerTv;
            }

            var index = inputUrl.IndexOf(marker);

            if (index == -1)
            {
                return 0;
            }

            var partAfterMarker = inputUrl.Substring(index + marker.Length);
            var numberPart = new string(partAfterMarker.TakeWhile(char.IsDigit).ToArray());
            if (int.TryParse(numberPart, out int movieId))
            {
                return movieId;
            }
            else
            {
                return 0;
            }
        }

        private static bool IsTmdbLink(string input)
        {
            if (input.Contains("themoviedb.org/movie/") || input.Contains("themoviedb.org/tv/"))
            {
                return true;
            }
            return false;
        }

        private static ContentType ContentTypeCheck(string input)
        {
            if (input.Contains("themoviedb.org/movie/"))
            {
                return ContentType.movie;
            }
            else if (input.Contains("themoviedb.org/tv/"))
            {
                return ContentType.tv;
            }
            return ContentType.Unknown;
        }
    }
}
