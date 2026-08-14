using CineMatch.Api.Enums;
using System.ComponentModel.DataAnnotations;

namespace CineMatch.Api.Data.DTO.RequestsDto.Movie
{
    public class SearchMovieRequest
    {
        [Required]
        public required string MainInput { get; set; }
        public ContentType Type { get; set; } = ContentType.Unknown;
        public int? Year { get; set; }
    }
}
