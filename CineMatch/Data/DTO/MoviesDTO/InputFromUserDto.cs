using CineMatch.Enums;

namespace CineMatch.Data.DTO.MoviesDTO
{
    public class InputFromUserDto
    {
        public string MainInput { get; set; } = null!;
        public  ContentType Type { get; set; } = ContentType.Unknown;
        public int? Year { get; set; }
    }
}
