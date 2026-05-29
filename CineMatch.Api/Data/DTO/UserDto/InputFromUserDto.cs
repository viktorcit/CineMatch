using CineMatch.Api.Enums;

namespace CineMatch.Api.Data.DTO.UserDto
{
    public class InputFromUserDto
    {
        public string MainInput { get; set; } = null!;
        public  ContentType Type { get; set; } = ContentType.Unknown;
        public int? Year { get; set; }
    }
}
