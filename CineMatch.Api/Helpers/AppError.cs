using CineMatch.Api.Enums;

namespace CineMatch.Api.Helpers
{
    public sealed record AppError(
        ErrorType ErrorType,
        string ResponseMessage);
}
