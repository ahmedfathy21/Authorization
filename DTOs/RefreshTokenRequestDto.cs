namespace AuthSystemAPI.DTOs
{
    /// <summary>
    /// DTO for refresh token requests
    /// </summary>
    public class RefreshTokenRequestDto
    {
        /// <summary>
        /// Refresh token string
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
    }
}
