using Microsoft.AspNetCore.Identity;
using AuthSystemAPI.Entities;
namespace AuthSystemAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Firstname { get; set; }
        public string Lastname { get; set;}


        // Example: If you want to track if a user is active
        public bool IsActive { get; set; } = true;

        // Refresh tokens for JWT authentication
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        // Login sessions for auditing
        public ICollection<LoginUser> LoginUsers { get; set; } = new List<LoginUser>();
    }
}