namespace CineMatch.Model
{
    public class SessionParticipant
    {
        public int Id { get; set; }

        public int SessionId { get; set; }

        public Session Session { get; set; } = null!;

        public string ClientId { get; set; } = null!;

        public int ParticipantNumber { get; set; }
    }
}
