namespace CineMatch.Api.Data.DTO.MoviesDTO
{
    public class SearchMovieDto
    {
        public required string Title { get; set; } = null!;
        public int? Year { get; set; }
    }
}
