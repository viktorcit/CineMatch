
namespace CineMatch.Api.Entity
{
    public class Vote
    {
        public int Id { get; set; }
        public required int SessionId { get; set; }
        public required int MovieId { get; set; }
        public required bool IsLiked { get; set; }
        public required string ParticipantId { get; set; }
    }
}
