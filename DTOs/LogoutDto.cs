namespace AuthSystemAPI.DTOs
{
    /// <summary>
    /// DTO for logout requests
    /// </summary>
    public class LogoutDto
    {
        /// <summary>
        /// Refresh token to revoke
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
    }
}
