using System.ComponentModel.DataAnnotations;

namespace CineMatch.Api.Data.DTO.RequestsDto.Tokens
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public required string OldRefreshToken { get; set; }
    }
}
