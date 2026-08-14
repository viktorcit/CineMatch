using System.ComponentModel.DataAnnotations;

namespace CineMatch.Api.Data.DTO.RequestsDto.Session
{
    public class JoinSessionRequestDto
    {
        [Required]
        public required string Code { get; set; }
    }
}
 