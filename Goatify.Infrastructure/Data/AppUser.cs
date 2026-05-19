using Microsoft.AspNetCore.Identity;

namespace Goatify.Infrastructure
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
    }
}
