using Microsoft.AspNetCore.Identity;

namespace CineMatch.Api.Entity
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
