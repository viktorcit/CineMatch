using CineMatch.Api.Enums;
using Microsoft.AspNetCore.Identity;

namespace CineMatch.Api.Helpers
{
    public static class IdentityErrorHandler
    {
        public static AppError ErrorHandler(IdentityError? error)
        {
            if (error == null)
            {
                return new AppError(
                    ErrorType.ServerError,
                    ResponseMessages.ServerError);
            }
            return error.Code switch
            {
                "DuplicateUserName" => new AppError(
                    ErrorType.Conflict,
                    "Username is already taken."),

                "DuplicateEmail" => new AppError(
                    ErrorType.Conflict,
                    "Email is already registered."),

                "ConcurrencyFailure" => new AppError(
                    ErrorType.Conflict,
                    "Error in concurrency execution"),



                "InvalidUserName" => new AppError(
                    ErrorType.BadRequest,
                    "Username is invalid."),

                "PasswordTooShort" => new AppError(
                    ErrorType.BadRequest,
                    "Password is too short."),

                "PasswordRequiresUniqueChars" => new AppError(
                    ErrorType.BadRequest,
                    "Password does not contain enough distinct characters."),

                "PasswordMismatch" => new AppError(
                    ErrorType.BadRequest,
                    "The current password is incorrect."),

                "InvalidEmail" => new AppError(
                    ErrorType.BadRequest,
                    "Email is invalid."),

                "PasswordRequiresNonAlphanumeric" => new AppError(
                    ErrorType.BadRequest,
                    "Password must contain at least one non-alphanumeric character."),

                "PasswordRequiresDigit" => new AppError(
                    ErrorType.BadRequest,
                    "Password must contain at least one digit."),

                "PasswordRequiresLower" => new AppError(
                    ErrorType.BadRequest,
                    "Password must contain at least one lowercase letter."),

                "PasswordRequiresUpper" => new AppError(
                    ErrorType.BadRequest,
                    "Password must contain at least one uppercase letter."),

                _ => new AppError(
                    ErrorType.ServerError,
                    ResponseMessages.ServerError)
            };
        }
    }
}
