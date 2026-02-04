using AuthSystemAPI.Models;

namespace AuthSystemAPI.Entities
{
    /// <summary>
    /// Refresh token entity for JWT rotation
    /// </summary>
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Revoked { get; set; }
        public string? ReplacedByToken { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = default!;

        public bool IsActive => Revoked == null && Expires > DateTime.UtcNow;
    }
}
