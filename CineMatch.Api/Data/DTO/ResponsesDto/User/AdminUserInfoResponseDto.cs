namespace CineMatch.Api.Data.DTO.ResponsesDto.User
{
    public class AdminUserInfoResponseDto
    {
        //User info
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public required List<string> UserRoles { get; set; } = [];
        public DateTime? CreatedAt { get; set; }

        //User session info
        public int? SessionId { get; set; }
        public int? UserParticipantNumber { get; set; }
        public string? SessionCode { get; set; }

        //User session movie info
        public List<int>? SessionMovieId { get; set; }
    }
}
