using System.ComponentModel.DataAnnotations;

namespace CineMatch.Api.Data.DTO.RequestsDto.Session
{
    public class VoteRequestDto
    {
        [Required]
        public required int MovieId { get; set; }
    }
}
