using CineMatch.Enums;

namespace CineMatch.Data.DTO
{
    public class BaseResponseDto
    {
        public bool IsSuccess { get; set; }
        public string ResponseMessage { get; set; } = null!;
        public string? Errors { get; set; }
        public ErrorType ErrorType { get; set; }
    }
}
