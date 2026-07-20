using System.ComponentModel.DataAnnotations;

namespace CineMatch.Api.Data.DTO.TokensDto
{
    public class AccessTokenDto
    {
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string UserId { get; set; } = string.Empty;
        public IReadOnlyCollection<string> UserRoles { get; set; } = [];
    }
}
