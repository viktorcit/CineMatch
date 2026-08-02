namespace CineMatch.Api.Data.Contracts
{
    public class TokensRefreshInfo
    {
        public required string UserName { get; set; }
        public required string UserId { get; set; }
        public required string OldRefreshToken { get; set; }
        public required IReadOnlyCollection<string> UserRoles { get; set; } = [];
    }
}
