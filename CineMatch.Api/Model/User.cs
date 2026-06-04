namespace CineMatch.Api.Model
{
    public class User
    {
        public int Id { get; set; }
        public required string PublicId { get; set; } = null!;
        public required string Secret { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
