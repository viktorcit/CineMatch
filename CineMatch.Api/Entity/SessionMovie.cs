
namespace CineMatch.Api.Entity
{
    public class SessionMovie
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public required Movie Movie { get; set; }
        public int MovieId { get; set; }
    }
}
