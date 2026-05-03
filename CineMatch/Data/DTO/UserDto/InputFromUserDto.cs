using CineMatch.Enums;

namespace CineMatch.Data.DTO.UserDto
{
    public class InputFromUserDto
    {
        public string MainInput { get; set; } = null!;
        public  ContentType Type { get; set; } = ContentType.Unknown;
        public int? Year { get; set; }
    }
}
