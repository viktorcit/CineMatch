namespace CineMatch.Model
{
    public class Movie
    {
        public int Id { get; set; }
        public required string Title { get; set; } = null!;
        public int? Year { get; set; }
    }
}
