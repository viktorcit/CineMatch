namespace CineMatch.Api.Data.DTO.UserDto
{
    public class UserDto
    {
        public int Id { get; set; }
        public string PublicId { get; set; } = null!;
        public string Secret { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
