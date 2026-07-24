using CineMatch.Api.Data.DTO;
using CineMatch.Api.Enums;

namespace CineMatch.Api.Helpers
{
    public static class ErrorFactory
    {
        public static BaseResponseDto<T> Ok<T>(T? data, string message)
        {
            return new BaseResponseDto<T>
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = message,
                Errors = [],
                Data = data
            };
        }

        public static BaseResponseDto<T> NoContent<T>(string message)
        {
            return new BaseResponseDto<T>
            {
                IsSuccess = true,
                ErrorType = ErrorType.NoContent,
                ResponseMessage = message,
                Errors = [],
                Data = default
            };
        }

        public static BaseResponseDto<T> ServerError<T>(List<string> errorMessage)
        {
            return new BaseResponseDto<T>
            {
                IsSuccess = false,
                ErrorType = ErrorType.ServerError,
                ResponseMessage = "Something went wrong. There is a server-side problem please try again later.",
                Errors = errorMessage,
                Data = default
            };
        }

        public static BaseResponseDto<T> Unauthorized<T>()
        {
            return new BaseResponseDto<T>
            {
                IsSuccess = false,
                ErrorType = ErrorType.Unauthorized,
                ResponseMessage = "Unauthorized access. Please check your credentials.",
                Errors = [],
                Data = default
            };
        }

        public static BaseResponseDto<T> NotFound<T>(string message)
        {
            return new BaseResponseDto<T>
            {
                IsSuccess = false,
                ErrorType = ErrorType.NotFound,
                ResponseMessage = message,
                Errors = [],
                Data = default
            };
        }

        public static BaseResponseDto<T> BadRequest<T>(string message)
        {
            return new BaseResponseDto<T>
            {
                IsSuccess = false,
                ErrorType = ErrorType.BadRequest,
                ResponseMessage = message,
                Errors = [],
                Data = default
            };
        }

        public static BaseResponseDto<T> Conflict<T>(string message)
        {
            return new BaseResponseDto<T>
            {
                IsSuccess = false,
                ErrorType = ErrorType.Conflict,
                ResponseMessage = message,
                Errors = [],
                Data = default
            };
        }

        public static BaseResponseDto<T> TooManyRequests<T>(string message)
        {
            return new BaseResponseDto<T>
            {
                IsSuccess = false,
                ErrorType = ErrorType.TooManyRequests,
                ResponseMessage = message,
                Errors = [],
                Data = default
            };
        }

        public static BaseResponseDto<T> Timeout<T>(string message)
        {
            return new BaseResponseDto<T>
            {
                IsSuccess = false,
                ErrorType = ErrorType.Timeout,
                ResponseMessage = message,
                Errors = [],
                Data = default
            };
        }
    }
}
