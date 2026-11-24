using CineMatch.Api.Data.Contracts;
using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Enums;
using CineMatch.Api.Helpers;
using CineMatch.Api.Services.Interfaces.IMovieServices;

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



        public async Task<BaseResponseDto<MovieInfo>> GetMovieByUrlAsync(string inputUrl, CancellationToken cancellationToken)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "GetMovieByUrlAsync" });
            if (string.IsNullOrWhiteSpace(inputUrl))
            {
                _logger.LogDebug("Operation rejected: {Reason}", "Input cannot be null.");
                return ResponseFactory.Fail<MovieInfo>(ErrorType.BadRequest, "Input cannot be null.");
            }
            if (!IsTmdbLink(inputUrl))
            {
                return ResponseFactory.Fail<MovieInfo>(ErrorType.BadRequest, ResponseMessages.InvalidTmdbUrl);
            }

            try
            {
                var movieId = ExtractIdFromLink(inputUrl);
                if (movieId == 0)
                {
                    return ResponseFactory.Fail<MovieInfo>(ErrorType.BadRequest, ResponseMessages.InvalidTmdbUrl);
                }

                var contentType = ContentTypeCheck(inputUrl);
                if (contentType == ContentType.Unknown)
                {
                    return ResponseFactory.Fail<MovieInfo>(ErrorType.BadRequest, ResponseMessages.InvalidTmdbUrl);
                }

                var movieDetails = await _tmdbService.GetMovieDetailsAsync(movieId, contentType, cancellationToken);
                if (movieDetails == null)
                {
                    _logger.LogDebug("Operation rejected: {Reason}", "Movie not found");
                    return ResponseFactory.Fail<MovieInfo>(ErrorType.NotFound, "Movie not found");
                }

                _logger.LogDebug("Found TMDb item {TmdbId} ({ContentType}) from link", movieId, contentType);
                return ResponseFactory.Ok(movieDetails);
            }
            catch (Exception ex)
            {
                _logger.Log(ex is OperationCanceledException && cancellationToken.IsCancellationRequested ? LogLevel.Debug : LogLevel.Error, ex, "Movie lookup by TMDb link failed");

                return ResponseFactory.Fail<MovieInfo>(ErrorType.ServerError, ResponseMessages.ServerError);
            }
        }

        public async Task<BaseResponseDto<List<MovieInfo>>> GetMovieBySearchAsync(string mainInput, ContentType inputContentType, int? inputYear, CancellationToken cancellationToken)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["Operation"] = "GetMovieBySearchAsync" });
            if (string.IsNullOrWhiteSpace(mainInput))
            {
                _logger.LogDebug("Operation rejected: {Reason}", "Input cannot be null");
                return ResponseFactory.Fail<List<MovieInfo>>(ErrorType.BadRequest, "Input cannot be null");
            }

            try
            {
                var movieDetails = await _tmdbService.GetMovieDetailsFromSearchAsync(mainInput, inputContentType, inputYear, cancellationToken);
                if (movieDetails == null)
                {
                    _logger.LogDebug("Operation rejected: {Reason}", "Movie not found.");
                    return ResponseFactory.Fail<List<MovieInfo>>(ErrorType.BadRequest, "Movie not found.");
                }

                _logger.LogDebug("Movie search returned {MovieCount} results for content type {ContentType} and year {Year}", movieDetails.Count, inputContentType, inputYear);
                return ResponseFactory.Ok(movieDetails);
            }
            catch (Exception ex)
            {
                _logger.Log(ex is OperationCanceledException && cancellationToken.IsCancellationRequested ? LogLevel.Debug : LogLevel.Error, ex, "Movie search failed for content type {ContentType} and year {Year}", inputContentType, inputYear);

                return ResponseFactory.Fail<List<MovieInfo>>(ErrorType.ServerError, ResponseMessages.ServerError);
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
