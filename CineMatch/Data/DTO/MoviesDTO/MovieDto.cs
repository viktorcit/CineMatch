namespace CineMatch.Data.DTO.MoviesDto
{
    public class MovieDto
    {
        public int Id { get; set; }
        public required string Title { get; set; } = null!;
        public int? Year { get; set; }
        public string Overview { get; set; } = null!;
        public string PosterUrl { get; set; } = null!;
        public List<string?> Genres { get; set; } = new();
    }
}
