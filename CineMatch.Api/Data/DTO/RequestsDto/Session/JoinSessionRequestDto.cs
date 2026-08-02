namespace CineMatch.Api.Data.DTO.RequestsDto.Session
{
    public class JoinSessionRequestDto
    {
        public required string Code { get; set; }
        public required string ClientId { get; set; }
    }
}
