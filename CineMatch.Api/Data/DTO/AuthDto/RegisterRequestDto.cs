using System.ComponentModel.DataAnnotations;

namespace CineMatch.Api.Data.DTO.AuthDto
{
    public class RegisterRequestDto
    {
        [Required]
        [MinLength(5), MaxLength(15)]
        public string UserName { get; set; } = string.Empty;
        [Required]
        [MinLength(6), MaxLength(20)]
        public string Password { get; set; } = string.Empty;
    }
}
