namespace AuthSystemAPI.DTOs
{
    /// <summary>
    /// DTO for returning user data in API responses
    /// Used in Get, List, Create, and Update endpoints
    /// Never exposes sensitive data like password hashes
    /// </summary>
    public class UserResponseDto
    {
        /// <summary>
        /// Unique user identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Email address
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
        /// User's phone number
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Whether email is confirmed
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// Whether phone number is confirmed
        /// </summary>
        public bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        /// Whether the user account is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Date the user was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// List of roles assigned to this user
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();
    }
}
