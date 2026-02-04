namespace AuthSystemAPI.DTOs
{
    /// <summary>
    /// DTO for authentication responses
    /// Contains access and refresh tokens
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>
        /// JWT access token
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Refresh token for renewing access
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Access token expiration (UTC)
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
}
