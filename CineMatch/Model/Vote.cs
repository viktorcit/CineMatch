namespace CineMatch.Model
{
    public class Vote
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;
        public bool IsLiked { get; set; }
        public int ParticipantNumber { get; set; }
    }
}
