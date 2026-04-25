namespace CineMatch.Data.DTO
{
    public class BaseResponseWithDataDto<T> : BaseResponseDto
    {
        public T? Data { get; set; }
    }
}
