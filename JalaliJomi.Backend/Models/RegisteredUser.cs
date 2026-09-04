using Microsoft.AspNetCore.Identity;

namespace JalaliJomi.Backend.Models
{
    public class RegisteredUser : IdentityUser<int>
    {
        // Id, Email, PasswordHash, PhoneNumber già forniti da IdentityUser<int>

        public string FullName { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}