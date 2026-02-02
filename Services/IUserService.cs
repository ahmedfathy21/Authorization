using AuthSystemAPI.DTOs;

namespace AuthSystemAPI.Services
{
    /// <summary>
    /// Interface for User Service
    /// Defines all user management operations
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="createUserDto">User creation data</param>
        /// <returns>Created user response</returns>
        Task<ApiResponseDto<UserResponseDto>> CreateUserAsync(CreateUserDto createUserDto);

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User details</returns>
        Task<ApiResponseDto<UserResponseDto>> GetUserByIdAsync(string userId);

        /// <summary>
        /// Get user by username
        /// </summary>
        /// <param name="userName">Username</param>
        /// <returns>User details</returns>
        Task<ApiResponseDto<UserResponseDto>> GetUserByUserNameAsync(string userName);

        /// <summary>
        /// Get all users with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <param name="searchTerm">Optional search term for filtering</param>
        /// <returns>Paginated list of users</returns>
        Task<ApiResponseDto<PaginatedResultDto<UserResponseDto>>> GetAllUsersAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string searchTerm = null);

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="updateUserDto">Updated user data</param>
        /// <returns>Updated user response</returns>
        Task<ApiResponseDto<UserResponseDto>> UpdateUserAsync(string userId, UpdateUserDto updateUserDto);

        /// <summary>
        /// Delete a user (soft delete by setting IsActive to false)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Success/failure response</returns>
        Task<ApiResponseDto<bool>> DeleteUserAsync(string userId);

        /// <summary>
        /// Permanently delete a user from the database
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Success/failure response</returns>
        Task<ApiResponseDto<bool>> PermanentlyDeleteUserAsync(string userId);

        /// <summary>
        /// Assign role(s) to a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roles">List of role names</param>
        /// <returns>Success/failure response</returns>
        Task<ApiResponseDto<bool>> AssignRolesToUserAsync(string userId, List<string> roles);

        /// <summary>
        /// Remove role(s) from a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roles">List of role names</param>
        /// <returns>Success/failure response</returns>
        Task<ApiResponseDto<bool>> RemoveRolesFromUserAsync(string userId, List<string> roles);

        /// <summary>
        /// Get all roles assigned to a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of role names</returns>
        Task<ApiResponseDto<List<string>>> GetUserRolesAsync(string userId);
    }
}
