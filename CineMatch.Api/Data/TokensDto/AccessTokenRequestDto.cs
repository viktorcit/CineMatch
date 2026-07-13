using System.ComponentModel.DataAnnotations;

namespace CineMatch.Api.Data.TokensDto
{
    public class AccessTokenRequestDto
    {
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string UserId { get; set; } = string.Empty;
        public string? OldRefreshToken { get; set; }
        public IReadOnlyCollection<string> UserRoles { get; set; } = [];
    }
}
