
namespace CineMatch.Api.Data.DTO.UserDto
{
    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
