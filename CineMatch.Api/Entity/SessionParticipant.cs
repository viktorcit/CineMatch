
namespace CineMatch.Api.Entity
{
    public class SessionParticipant
    {
        public int Id { get; set; }
        public required int SessionId { get; set; }
        public required string UserId { get; set; }
        public required int ParticipantNumber { get; set; }
    }
}
