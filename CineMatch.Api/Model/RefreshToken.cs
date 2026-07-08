namespace CineMatch.Api.Model
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? RevokedAt { get; set; }
        public bool IsRevoked { get; set; } = false;
        public string? ReplacedByToken { get; set; } = string.Empty;
    }
}
