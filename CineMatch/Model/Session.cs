namespace CineMatch.Model
{
    public class Session
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string Code { get; set; } = null!;
        public string CreatorClientId { get; set; } = null!;
        public List<SessionMovie> SessionMovies { get; set; } = [];
        public List<Vote> Votes { get; set; } = [];
        public List<SessionParticipant> Participants { get; set; } = [];
    }   
}
