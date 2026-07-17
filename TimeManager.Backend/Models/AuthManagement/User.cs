using Microsoft.AspNetCore.Identity;

namespace TimeManager.Backend.Models.AuthManagement
{
    public class User: IdentityUser<int>
    {
        public Preferences Preferences { get; set; } = new Preferences();
    }
}
