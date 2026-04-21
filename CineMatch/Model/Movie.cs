namespace CineMatch.Model
{
    public class Movie
    {
        public int Id { get; set; }
        public required string Title { get; set; } = null!; 
        public required string Genre { get; set; } = null!;
        public int ReleaseYear { get; set; }
        public required string Discription { get; set; } = null!;
    }
}
