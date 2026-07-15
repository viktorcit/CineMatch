namespace CineMatch.Api.Data.DTO.TokensDto
{
    public class RefreshTokenRequestDto
    {
        public string OldRefreshToken { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
