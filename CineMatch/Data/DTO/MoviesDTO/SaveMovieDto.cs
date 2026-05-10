using CineMatch.Enums;
using CineMatch.Model;

namespace CineMatch.Data.DTO.MoviesDTO
{
    public class SaveMovieDto
    {
        public int SessionId { get; set; }
        public int MovieId { get; set; }

        public int TMdbId { get; set; }
        public ContentType Type { get; set; }
        public required string Title { get; set; } = null!;
        public int? Year { get; set; }
        public string Overview { get; set; } = null!;
        public string PosterUrl { get; set; } = null!;
        public List<string?> Genres { get; set; } = new();
    }
}
