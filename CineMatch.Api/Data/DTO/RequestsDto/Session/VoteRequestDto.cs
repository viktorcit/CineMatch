using System.ComponentModel.DataAnnotations;

namespace CineMatch.Api.Data.DTO.RequestsDto.Session
{
    public class VoteRequestDto
    {
        public required string ClientId { get; set; }
        [Required]
        public int MovieId { get; set; }
    }
}
