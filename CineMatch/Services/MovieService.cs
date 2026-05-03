using CineMatch.Data.DTO;
using CineMatch.Data.DTO.MoviesDto;
using CineMatch.Enums;
using CineMatch.Model;
using CineMatch.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CineMatch.Services
{
    public class MovieService : IMovieService
    {
        private readonly HttpClient _httpClient;
        private readonly string _token;
        private readonly ILogger<MovieService> _logger;
        public MovieService(HttpClient httpClient, IConfiguration config, ILogger<MovieService> logger)
        {
            _httpClient = httpClient;
            _token = config["Tmdb:ApiToken"]
                ?? throw new InvalidOperationException("TMDb token not configured");
            _logger = logger;
        }



        public async Task<BaseResponseWithDataDto<MovieDto>> GetMovieByUrlAsync(string inputUrl)
        {

            if (string.IsNullOrWhiteSpace(inputUrl))
            {
                _logger.LogInformation("Поле ввода пустое");
                return new BaseResponseWithDataDto<MovieDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Input cannot be null"
                };
            }
            if (!IsTmdbLink(inputUrl))
            {
                _logger.LogInformation("неправильная ссылка");
                return new BaseResponseWithDataDto<MovieDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Input URl is not supported."
                };
            }

            try
            {
                var movieId = ExtractIdFromLink(inputUrl);
                if (movieId == 0)
                {
                    _logger.LogInformation("неправильная ссылка");
                    return new BaseResponseWithDataDto<MovieDto>
                    {
                        IsSuccess = false,
                        ErrorType = ErrorType.BadRequest,
                        ResponseMessage = "Invalid TMDb URL format."
                    };
                }

                var contentType = ContentTypeCheck(inputUrl);
                if (contentType == ContentType.Unknown)
                {
                    _logger.LogInformation("неправильная ссылка");
                    return new BaseResponseWithDataDto<MovieDto>
                    {
                        IsSuccess = false,
                        ErrorType = ErrorType.BadRequest,
                        ResponseMessage = "Unsupported TMDb URL format."
                    };
                }

                var movieDetails = await GetMovieDetails(movieId, contentType);
                if (movieDetails == null)
                {
                    _logger.LogInformation("фильм не найден");
                    return new BaseResponseWithDataDto<MovieDto>
                    {
                        IsSuccess = false,
                        ErrorType = ErrorType.NotFound,
                        ResponseMessage = "Movie not found."
                    };
                }
                _logger.LogInformation("фильм найден");
                return new BaseResponseWithDataDto<MovieDto>
                {
                    IsSuccess = true,
                    ErrorType = ErrorType.None,
                    ResponseMessage = "Movie details fetched successfully.",
                    Data = movieDetails
                };
            }
            catch (Exception ex)
            {
                _logger.LogInformation("произошла какая то ошибка");
                return new BaseResponseWithDataDto<MovieDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.ServerError,
                    ResponseMessage = $"An error occurred while fetching movie details: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponseWithDataDto<List<MovieDto>>> GetMovieBySearchAsync(string mainInput, ContentType inputContentType, int? inputYear)
        {
            if (string.IsNullOrWhiteSpace(mainInput))
            {
                _logger.LogInformation("Поле ввода пустое");
                return new BaseResponseWithDataDto<List<MovieDto>>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Input cannot be null."
                };
            }

            try
            {
                var movieDetails = await GetMovieDetailsFromSearch(mainInput, inputContentType, inputYear);
                if (movieDetails == null)
                {
                    _logger.LogInformation("фильм не найден");
                    return new BaseResponseWithDataDto<List<MovieDto>>
                    {
                        IsSuccess = false,
                        ErrorType = ErrorType.NotFound,
                        ResponseMessage = "Movie not found."
                    };
                }
                _logger.LogInformation("фильм найден");
                return new BaseResponseWithDataDto<List<MovieDto>>
                {
                    IsSuccess = true,
                    ErrorType = ErrorType.None,
                    ResponseMessage = "Movie details fetched successfully.",
                    Data = movieDetails
                };
            }
            catch (Exception ex)
            {
                _logger.LogInformation("произошла какая то ошибка");
                return new BaseResponseWithDataDto<List<MovieDto>>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.ServerError,
                    ResponseMessage = $"An error occurred while searching for the movie: {ex.Message}"
                };
            }
        }


        //private methods
        private int ExtractIdFromLink(string inputUrl)
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

        private bool IsTmdbLink(string input)
        {
            if (input.Contains("themoviedb.org/movie/") || input.Contains("themoviedb.org/tv/"))
            {
                return true;
            }
            return false;
        }

        private ContentType ContentTypeCheck(string input)
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

        private async Task<List<MovieDto>?> GetMovieDetailsFromSearch(string title, ContentType type, int? year)
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

            var movies = new List<MovieDto>();

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

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            if(!doc.RootElement.TryGetProperty("results", out var resultsArray))
            {
                return null;
            }

            var resultsList = resultsArray
                .EnumerateArray()
                .Select(r => new SearchResult
                {
                    MovieId = r.GetProperty("id").GetInt32(),
                    Type = r.TryGetProperty("media_type", out var typeProp)
                    ? (typeProp.GetString() == "tv" ? ContentType.tv : ContentType.movie)
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


        private async Task<MovieDto?> GetMovieDetails(int movieId, ContentType type)
        {
            var url = $"https://api.themoviedb.org/3/{type}/{movieId}?language=ru-RU";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

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

            int? year = !string.IsNullOrEmpty(dateString)
                ? DateTime.Parse(dateString).Year
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

            var genres = doc.RootElement.GetProperty("genres").EnumerateArray()
                .Select(g => g.GetProperty("name").GetString())
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .ToList();
            if (genres.Count == 0)
            {
                genres.Add("Unknown");
            }

            var movie = new Movie
            {
                Title = title,
                Year = year,
                Overview = overview,
                PosterUrl = posterUrl,
                Genres = genres
            };

            var responseMovie = new MovieDto
            {
                Title = title,
                Year = year,
                Overview = overview,
                PosterUrl = posterUrl,
                Genres = genres
            };
            return responseMovie;
        }
    }
}
