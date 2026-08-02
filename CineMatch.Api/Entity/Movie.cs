using CineMatch.Api.Enums;

namespace CineMatch.Api.Entity
{
    public class Movie
    {
        public int Id { get; set; }
        public required int TMdbId { get; set; }
        public required ContentType Type { get; set; }
        public required string Title { get; set; } = null!;
        public int? Year { get; set; }
        public string Overview { get; set; } = null!;
        public string PosterUrl { get; set; } = string.Empty;
        public List<string?> Genres { get; set; } = [];
    }
}
