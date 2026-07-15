namespace CineMatch.Api.Data.DTO.AuthDto
{
    public class LoginRequestDto
    {
        public required string UserName { get; set; } = string.Empty;
        public required string Password { get; set; } = string.Empty;
    }
}
