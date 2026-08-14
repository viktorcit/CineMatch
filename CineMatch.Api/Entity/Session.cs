
namespace CineMatch.Api.Entity
{
    public class Session
    {
        public int Id { get; set; }
        public required string Code { get; set; }
        public required string CreatorUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }   
}
