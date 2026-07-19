namespace CineMatch.Api.Data.DTO.TokensDto
{
    public class RefreshTokenRequestDto
    {
        public required string OldRefreshToken { get; set; } = string.Empty;
        public required string UserId { get; set; } = string.Empty;
    }
}
