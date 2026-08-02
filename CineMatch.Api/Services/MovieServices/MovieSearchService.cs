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
        private readonly HttpClient _httpClient;
        private readonly string _tmdbApiToken;
        private readonly ILogger<MovieSearchService> _logger;
        public MovieSearchService
            (HttpClient httpClient,
            IConfiguration config,
            ILogger<MovieSearchService> logger)
        {
            _httpClient = httpClient;
            _tmdbApiToken = config["Tmdb:ApiToken"]
                ?? throw new InvalidOperationException("TMDb token not configured");
            _logger = logger;
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

                var movieDetails = await GetMovieDetails(movieId, contentType);
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
                var movieDetails = await GetMovieDetailsFromSearch(mainInput, inputContentType, inputYear);
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
        private async Task<List<SearchResult>?> SearchMovie(string inputTitle, ContentType inputType, int? inputYear)
        {
            if (string.IsNullOrWhiteSpace(inputTitle))
            {
                return null;
            }

            var endpoint = inputType == ContentType.tv ? "tv" : "movie";

            var url = $"https://api.themoviedb.org/3/search/{endpoint}?query={inputTitle}";

            if (inputYear.HasValue)
            {
                url += inputType == ContentType.movie
                    ? $"&year={inputYear}"
                    : $"&first_air_date_year={inputYear}";
            }

            var response = await SendRequest(url);
            if (response == null)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("results", out var resultsArray))
            {
                return null;
            }

            var resultsList = GetMoviesList(resultsArray, inputType);
            if(resultsList == null)
            {
                return null;
            }

            return resultsList;
        }


        private async Task<MovieInfo?> GetMovieDetails(int movieId, ContentType type)
        {
            var url = $"https://api.themoviedb.org/3/{type}/{movieId}?language=ru-RU";

            var response = await SendRequest(url);
            if(response == null)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            var movie = GetMovieData(type, doc, movieId);
            var responseMovie = CreateMovieDto(movie);

            return responseMovie;
        }

        private async Task<List<MovieInfo>?> GetMovieDetailsFromSearch(string title, ContentType type, int? year)
        {
            var searchResult = new List<SearchResult>();
            if (type != ContentType.Unknown)
            {
                searchResult = await SearchMovie(title, type, year);
                if (searchResult == null || searchResult.Count == 0)
                {
                    searchResult = await SearchMovie(title, type, null);
                }
            }
            else
            {
                searchResult = await SearchMovie(title, ContentType.movie, year);
                if (searchResult == null || searchResult.Count == 0)
                {
                    searchResult = await SearchMovie(title, ContentType.movie, null);
                }
                if (searchResult == null || searchResult.Count == 0)
                {
                    searchResult = await SearchMovie(title, ContentType.tv, year);
                }
                if (searchResult == null || searchResult.Count == 0)
                {
                    searchResult = await SearchMovie(title, ContentType.tv, null);
                }
            }

            if (searchResult == null || searchResult.Count == 0)
            {
                return null;
            }

            var movies = new List<MovieInfo>();

            foreach (var result in searchResult)
            {
                var details = await GetMovieDetails(result.MovieId, result.Type);
                if (details != null)
                {
                    movies.Add(details);
                }
            }
            if (movies.Count == 0)
            {
                return null;
            }

            return movies;
        }

        private async Task<HttpResponseMessage?> SendRequest(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tmdbApiToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return response;
        }

        //static private methods
        private static Movie GetMovieData(ContentType type, JsonDocument doc, int movieId)
        {
            var title = type == ContentType.movie
                ? doc.RootElement.GetProperty("title").GetString()
                : doc.RootElement.GetProperty("name").GetString();

            if (string.IsNullOrEmpty(title))
            {
                title = "Unknown";
            }

            var dateString = type == ContentType.movie
                ? doc.RootElement.GetProperty("release_date").GetString()
                : doc.RootElement.GetProperty("first_air_date").GetString();

            
            int? year = DateTime.TryParse(dateString, out var parsedData)
                ? parsedData.Year
                : null;

            var overview = doc.RootElement.GetProperty("overview").GetString();
            if (string.IsNullOrEmpty(overview))
            {
                overview = "Unknown";
            }

            var posterPath = doc.RootElement.GetProperty("poster_path").GetString();
            var posterUrl = string.IsNullOrEmpty(posterPath)
                ? string.Empty
                : $"https://image.tmdb.org/t/p/w500{posterPath}";

            var genres = doc.RootElement.GetProperty("genres")
                .EnumerateArray()
                .Select(g => g.GetProperty("name").GetString())
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .ToList();

            var movie = new Movie
            {
                Title = title,
                Year = year,
                Overview = overview,
                PosterUrl = posterUrl,
                Genres = genres,
                TMdbId = movieId,
                Type = type
            };

            return movie;
        }

        private static List<SearchResult>? GetMoviesList(JsonElement resultsArray, ContentType inputType)
        {
            var resultsList = resultsArray
                .EnumerateArray()
                .Select(r => new SearchResult
                {
                    MovieId = r.GetProperty("id").GetInt32(),
                    Type = r.TryGetProperty("media_type", out var typeProp)
                    ? typeProp.GetString() == "tv" ? ContentType.tv : ContentType.movie
                    : inputType
                })
                .Take(5)
                .ToList();
            if (resultsList.Count == 0)
            {
                return null;
            }
            return resultsList;
        }  

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

        private static MovieInfo CreateMovieDto(Movie movie)
        {
            var movieDto = new MovieInfo
            {
                Title = movie.Title,
                Year = movie.Year,
                Overview = movie.Overview,
                PosterUrl = movie.PosterUrl,
                Genres = movie.Genres,
                TMdbId = movie.TMdbId,
                Type = movie.Type
            };
            return movieDto;
        }
    }
}
