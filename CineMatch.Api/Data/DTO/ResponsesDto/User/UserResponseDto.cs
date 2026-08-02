namespace CineMatch.Api.Data.DTO.ResponsesDto.User
{
    public class UserResponseDto
    {
        public required string UserName { get; set; }
        public required List<string> UserRoles { get; set; } = [];
    }
}
