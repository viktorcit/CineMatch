using System.ComponentModel.DataAnnotations;

namespace CineMatch.Api.Data.DTO.RequestsDto.Auth
{
    public class LoginRequestDto
    {
        [MinLength(5), MaxLength(15)]
        public required string UserName { get; set; }
        [MinLength(6), MaxLength(20)]
        public required string Password { get; set; }    
    }
}
