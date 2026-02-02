namespace AuthSystemAPI.DTOs
{
    /// <summary>
    /// DTO for creating a new user
    /// Used in registration/user creation API endpoints
    /// </summary>
    public class CreateUserDto
    {
        /// <summary>
        /// User's username - must be unique
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// User's email address - must be unique
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// User's first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// User's password - will be hashed before storage
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Confirm password - must match Password field
        /// </summary>
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// User's phone number (optional)
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Whether the user account is active - default is true
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
