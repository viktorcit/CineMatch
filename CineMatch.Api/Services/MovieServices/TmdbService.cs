using CineMatch.Api.Data.Contracts;
using CineMatch.Api.Entity;
using CineMatch.Api.Enums;
using CineMatch.Api.Services.Interfaces.IMovieServices;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CineMatch.Api.Services.MovieServices
{
    public class TmdbService : ITmdbService
    {
        private readonly HttpClient _httpClient;
        private readonly string _tmdbApiToken;

        public TmdbService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _tmdbApiToken = config["Tmdb:ApiToken"]
                ?? throw new InvalidOperationException("TMDb token not configured");
        }

        public async Task<MovieInfo?> GetMovieDetailsAsync(int tmdbId, ContentType type)
        {
            var url = $"https://api.themoviedb.org/3/{type}/{tmdbId}?language=ru-RU";

            var response = await SendRequest(url);
            if (response == null)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            var movie = GetMovieData(type, doc, tmdbId);
            var responseMovie = CreateMovieDto(movie);

            return responseMovie;
        }

        public async Task<List<MovieInfo>?> GetMovieDetailsFromSearchAsync(string title, ContentType type, int? year)
        {
            var searchResult = new List<SearchResult>();
            if (type != ContentType.Unknown)
            {
                searchResult = await SearchMovieAsync(title, type, year);
                if (searchResult == null || searchResult.Count == 0)
                {
                    searchResult = await SearchMovieAsync(title, type, null);
                }
            }
            else
            {
                searchResult = await SearchMovieAsync(title, ContentType.movie, year);
                if (searchResult == null || searchResult.Count == 0)
                {
                    searchResult = await SearchMovieAsync(title, ContentType.movie, null);
                }
                if (searchResult == null || searchResult.Count == 0)
                {
                    searchResult = await SearchMovieAsync(title, ContentType.tv, year);
                }
                if (searchResult == null || searchResult.Count == 0)
                {
                    searchResult = await SearchMovieAsync(title, ContentType.tv, null);
                }
            }

            if (searchResult == null || searchResult.Count == 0)
            {
                return null;
            }

            var movies = new List<MovieInfo>();

            foreach (var result in searchResult)
            {
                var details = await GetMovieDetailsAsync(result.TmdbId, result.Type);
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


        //private methods
        private async Task<List<SearchResult>?> SearchMovieAsync(string inputTitle, ContentType inputType, int? inputYear)
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
            if (resultsList == null)
            {
                return null;
            }

            return resultsList;
        }

        //private static methods
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

        //private static methods

        private static List<SearchResult>? GetMoviesList(JsonElement resultsArray, ContentType inputType)
        {
            var resultsList = resultsArray
                .EnumerateArray()
                .Select(r => new SearchResult
                {
                    TmdbId = r.GetProperty("id").GetInt32(),
                    Type = r.TryGetProperty("media_type", out var typeProp)
                    ? typeProp.GetString() == "tv" ? ContentType.tv : ContentType.movie
                    : inputType
                })
                .Take(10)
                .ToList();
            if (resultsList.Count == 0)
            {
                return null;
            }
            return resultsList;
        }
        private static Movie GetMovieData(ContentType type, JsonDocument doc, int movieId)
        {
            var title = type == ContentType.movie
                ? doc.RootElement.GetProperty("title").GetString()
                : doc.RootElement.GetProperty("name").GetString();

            if (string.IsNullOrWhiteSpace(title))
            {
                title = "Unknown";
            }

            string datePropertyName = type == ContentType.movie ? "release_date" : "first_air_date";
            string? dateString = doc.RootElement.TryGetProperty(datePropertyName, out var dateProp)
                    ? dateProp.GetString()
                    : null;
            if (string.IsNullOrWhiteSpace(dateString))
            {
                dateString = "Unknown";
            }

            int? year = DateTime.TryParse(dateString, out var parsedData)
                ? parsedData.Year
                : null;

            string? overview = doc.RootElement.TryGetProperty("overview", out var overviewProp)
                ? overviewProp.GetString()
                : null;
            if (string.IsNullOrWhiteSpace(overview))
            {
                overview = "Unknown";
            }

            var posterPath = doc.RootElement.TryGetProperty("poster_path", out var posterProp)
                ? posterProp.GetString()
                : null;
            var posterUrl = string.IsNullOrEmpty(posterPath)
                ? string.Empty
                : $"https://image.tmdb.org/t/p/w500{posterPath}";

            var genres = doc.RootElement.TryGetProperty("genres", out var genresProp)
                ? genresProp.EnumerateArray()
                .Select(g => g.TryGetProperty("name", out var nameProp)
                ? nameProp.GetString()
                : null)
                .Where(g => !string.IsNullOrWhiteSpace(g)).ToList()
                : [];

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
