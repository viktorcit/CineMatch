using System.ComponentModel.DataAnnotations;

namespace CineMatch.Api.Data.DTO.TokensDto
{
    public class TokensRefreshDto
    {
        public required string UserName { get; set; } = string.Empty;
        public required string UserId { get; set; } = string.Empty;
        public required string OldRefreshToken { get; set; } = string.Empty;
        public required IReadOnlyCollection<string> UserRoles { get; set; } = [];
    }
}
