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




        public async Task<BaseResponseWithDataDto<MovieDto>> GetMovieAsync(string input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input))
            {
                _logger.LogInformation("Ссылка пуста");
                return new BaseResponseWithDataDto<MovieDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Input URL cannot be null."
                };
            }

            if (IsTmdbLink(input))
            {
                try
                {
                    var movieId = ExtractIdFromLink(input);
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

                    var contentType = ContentTypeCheck(input);
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

            return new BaseResponseWithDataDto<MovieDto>
            {
                IsSuccess = false,
                ErrorType = ErrorType.BadRequest,
                ResponseMessage = "Unsupported URL format."
            };
        }


        //private mothods
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

        private async Task<MovieDto?> SearchMovie(string title)
        {
            var url = $"https://api.themoviedb.org/3/search/movie?query={title}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var results = doc.RootElement.GetProperty("results");
            if (results.GetArrayLength() == 0)
            {
                return null;
            }

            var firstMovie = results[0];
            var titleValue = firstMovie.GetProperty("title").GetString();
            if (titleValue == null)
            {
                return null;
            }
            var dateString = firstMovie.GetProperty("release_date").GetString();
            int? year = null;
            if (!string.IsNullOrEmpty(dateString))
            {
                year = DateTime.Parse(dateString).Year;
            }

            var movie = new Movie
            {
                Title = titleValue,
                Year = year
            };

            var responseMovie = new MovieDto
            {
                Title = movie.Title,
                Year = movie.Year
            };

            return responseMovie;
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

            string? title = null;
            if (type == ContentType.movie)
            {
                title = doc.RootElement.GetProperty("title").GetString();
            }
            else if (type == ContentType.tv)
            {
                title = doc.RootElement.GetProperty("name").GetString();
            }
            if (string.IsNullOrEmpty(title))
            {
                title = "Unknown";
            }

            string? dateString = null;
            if (type == ContentType.movie)
            {
                dateString = doc.RootElement.GetProperty("release_date").GetString();
            }
            else if (type == ContentType.tv)
            {
                dateString = doc.RootElement.GetProperty("first_air_date").GetString();
            }
            int? year = null;
            if (!string.IsNullOrEmpty(dateString))
            {
                year = DateTime.Parse(dateString).Year;
            }

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
