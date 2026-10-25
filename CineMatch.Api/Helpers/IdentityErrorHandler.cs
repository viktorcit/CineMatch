using CineMatch.Api.Enums;
using Microsoft.AspNetCore.Identity;

namespace CineMatch.Api.Helpers
{
    public static class IdentityErrorHandler
    {
        public static AppError ErrorHandler(IdentityError error)
        {
            return error.Code switch
            {
                "DuplicateUserName" => new AppError(
                    ErrorType.Conflict,
                    "Username is already taken."),

                "DuplicateEmail" => new AppError(
                    ErrorType.Conflict,
                    "Email is already registered."),

                "PasswordTooShort" => new AppError(
                    ErrorType.BadRequest,
                    "Password is too short."),

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
