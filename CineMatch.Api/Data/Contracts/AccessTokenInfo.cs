
namespace CineMatch.Api.Data.Contracts
{
    public class AccessTokenInfo
    {
        public string UserName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public IReadOnlyCollection<string> UserRoles { get; set; } = [];
    }
}
