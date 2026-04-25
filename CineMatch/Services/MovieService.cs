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




        public async Task<BaseResponseWithDataDto<MovieDto>> GetMovieAsync(string inputUrl)
        {
            if (inputUrl == null || string.IsNullOrWhiteSpace(inputUrl))
            {
                _logger.LogInformation("Ссылка пуста");
                return new BaseResponseWithDataDto<MovieDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Input URL cannot be null."
                };
            }

            if (IsTmdbLink(inputUrl))
            {
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

                    var movieDetails = await GetMovieDetails(movieId);
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
            if (input.Contains("themoviedb.org/movie/"))
            {
                return true;
            }
            return false;
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
            if(titleValue == null)
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


        private async Task<MovieDto?> GetMovieDetails(int movieId)
        {
            var url = $"https://api.themoviedb.org/3/movie/{movieId}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var title = doc.RootElement.GetProperty("title").GetString();
            if (title == null)
            {
                return null;
            }
            var dateString = doc.RootElement.GetProperty("release_date").GetString();
            int? year = null;
            if (!string.IsNullOrEmpty(dateString))
            {
                year = DateTime.Parse(dateString).Year;
            }
            var movie = new Movie
            {
                Title = title,
                Year = year
            };

            var responseMovie = new MovieDto
            {
                Title = movie.Title,
                Year = movie.Year
            };
            return responseMovie;
        }
    }
}
