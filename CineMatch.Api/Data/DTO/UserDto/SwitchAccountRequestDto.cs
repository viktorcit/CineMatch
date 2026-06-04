namespace CineMatch.Api.Data.DTO.UserDto
{
    public class SwitchAccountRequestDto
    {
        public string PublicId { get; set; } = null!;
        public string Secret { get; set; } = null!;
    }
}
