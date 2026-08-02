namespace CineMatch.Api.Data.DTO.ResponsesDto.Tokens
{
    public class TokensResponseDto
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }
}
