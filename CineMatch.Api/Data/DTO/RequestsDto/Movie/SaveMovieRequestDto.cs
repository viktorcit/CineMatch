using CineMatch.Api.Enums;
using System.ComponentModel.DataAnnotations;

namespace CineMatch.Api.Data.DTO.RequestsDto.Movie
{
    public class SaveMovieRequestDto
    {
        [Required]
        public int TmdbId { get; set; }
        [Required]
        public required ContentType Type { get; set; }
    }
}
