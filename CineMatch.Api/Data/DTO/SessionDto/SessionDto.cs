using CineMatch.Api.Model;

namespace CineMatch.Api.Data.DTO.SessionDto
{
    public class SessionDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Code { get; set; } = null!;
        public string CreatorClientId { get; set; } = null!;
        public List<SessionMovie> SessionMovies { get; set; } = [];
        public List<Vote> Votes { get; set; } = [];
    }
}
