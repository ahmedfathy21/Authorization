namespace AuthSystemAPI.DTOs
{
    /// <summary>
    /// DTO for login audit records
    /// </summary>
    public class LoginUserDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public DateTime LoggedInAt { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
