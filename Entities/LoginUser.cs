using AuthSystemAPI.Models;

namespace AuthSystemAPI.Entities
{
    /// <summary>
    /// Records successful login sessions for auditing
    /// </summary>
    public class LoginUser
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = default!;

        public DateTime LoggedInAt { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
