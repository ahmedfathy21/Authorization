namespace AuthSystemAPI.DTOs
{
    /// <summary>
    /// DTO for updating an existing user
    /// Used in user update API endpoints
    /// </summary>
    public class UpdateUserDto
    {
        /// <summary>
        /// User's first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// User's phone number
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Whether the user account is active
        /// </summary>
        public bool IsActive { get; set; }
    }
}
