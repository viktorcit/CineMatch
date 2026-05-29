namespace CineMatch.Api.Data.DTO.SessionDto
{
    public class VoteRequestDto
    {
        public string clientId { get; set; } = null!;
        public int? movieId { get; set; }
    }
}
