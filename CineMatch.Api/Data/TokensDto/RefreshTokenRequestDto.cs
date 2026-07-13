namespace CineMatch.Api.Data.TokensDto
{
    public class RefreshTokenRequestDto
    {
        public string OldRefreshToken { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
