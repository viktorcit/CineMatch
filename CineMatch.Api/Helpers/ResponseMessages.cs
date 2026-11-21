namespace CineMatch.Api.Helpers
{
    public class ResponseMessages
    {
        public const string ServerError = "Something went wrong. There is a server-side problem please try again later.";
        public const string Unauthorized = "Unauthorized access. Please check your credentials.";
        public const string InvalidTmdbUrl = "Invalid TMDb URL format.";
        public const string Conflict = "Conflict occurred. The requested resource already exists or there is a conflicting operation.";
    }
}
