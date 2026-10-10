using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Enums;

namespace CineMatch.Api.Helpers
{
    public static class ResponseFactory
    {
        public static BaseResponseDto Ok(string? message = null)
        {
            return new BaseResponseDto
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = message
            };
        }

        public static BaseResponseDto<T> Ok<T>(T data)
        {
            return new BaseResponseDto<T>
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                Data = data
            };
        }

        public static BaseResponseDto<T> Fail<T>(ErrorType errorType, string? message = null)
        {
            return new BaseResponseDto<T>
            {
                IsSuccess = false,
                ErrorType = errorType,
                ResponseMessage = message,
                Data = default
            };
        }

        public static BaseResponseDto Fail(ErrorType errorType, string? message = null)
        {
            return new BaseResponseDto
            {
                IsSuccess = false,
                ErrorType = errorType,
                ResponseMessage = message
            };
        }
    }
}
