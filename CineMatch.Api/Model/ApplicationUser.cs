using Microsoft.AspNetCore.Identity;

namespace CineMatch.Api.Model
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
