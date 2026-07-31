using CineMatch.Api.Enums;

namespace CineMatch.Api.Data.DTO
{
    public class BaseResponseDto
    {
        public bool IsSuccess { get; set; }
        public ErrorType ErrorType { get; set; }
        public string ResponseMessage { get; set; } = null!;
    }

    public class BaseResponseDto<T> : BaseResponseDto
    {
        public T? Data { get; set; }
    }
}
