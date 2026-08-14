using System.Security.Claims;

namespace CineMatch.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException(
                "Authenticated user has no identifier claim.");
        }
    }
}
