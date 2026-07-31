using CineMatch.Api.Data.DTO;
using CineMatch.Api.Enums;

namespace CineMatch.Api.Helpers
{
    public static class ErrorFactory
    {
        public static BaseResponseDto Ok(string message)
        {
            return new BaseResponseDto
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = message
            };
        }

        public static BaseResponseDto<T> Ok<T>(T? data, string message)
        {
            return new BaseResponseDto<T>
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = message,
                Data = data
            };
        }

        public static BaseResponseDto<T> Fail<T>(ErrorType errorType, string message)
        {
            return new BaseResponseDto<T>
            {
                IsSuccess = true,
                ErrorType = errorType,
                ResponseMessage = message,
                Data = default
            };
        }

        public static BaseResponseDto Fail(ErrorType errorType, string message)
        {
            return new BaseResponseDto
            {
                IsSuccess = true,
                ErrorType = errorType,
                ResponseMessage = message
            };
        }
    }
}
