namespace CineMatch.Api.Data.DTO.AuthDto
{
    public class TokensResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
