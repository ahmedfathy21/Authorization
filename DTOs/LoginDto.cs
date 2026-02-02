namespace AuthSystemAPI.DTOs
{
    /// <summary>
    /// DTO for user login requests
    /// Used in authentication/login endpoints
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Username or email address
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// User's password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Whether to keep the user logged in (remember me)
        /// </summary>
        public bool RememberMe { get; set; } = false;
    }
}
