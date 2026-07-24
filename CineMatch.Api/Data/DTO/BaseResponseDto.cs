using CineMatch.Api.Enums;

namespace CineMatch.Api.Data.DTO
{
    public class BaseResponseDto<T>
    {
        public bool IsSuccess { get; set; }
        public ErrorType ErrorType { get; set; }
        public string ResponseMessage { get; set; } = null!;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } =  [];
    }
}
