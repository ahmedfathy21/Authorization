using AuthSystemAPI.DTOs;
using AuthSystemAPI.Models;
using AuthSystemAPI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthSystemAPI.Services
{
    /// <summary>
    /// Service for managing user operations
    /// Implements business logic for user management
    /// </summary>
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        public async Task<ApiResponseDto<UserResponseDto>> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                // Validate passwords match
                if (createUserDto.Password != createUserDto.ConfirmPassword)
                {
                    return new ApiResponseDto<UserResponseDto>("Password and Confirm Password do not match", 
                        new List<string> { "Passwords must match" });
                }

                // Check if username already exists
                var existingUser = await _userManager.FindByNameAsync(createUserDto.UserName);
                if (existingUser != null)
                {
                    return new ApiResponseDto<UserResponseDto>("Username already exists", 
                        new List<string> { $"User with username '{createUserDto.UserName}' already exists" });
                }

                // Check if email already exists
                existingUser = await _userManager.FindByEmailAsync(createUserDto.Email);
                if (existingUser != null)
                {
                    return new ApiResponseDto<UserResponseDto>("Email already exists", 
                        new List<string> { $"User with email '{createUserDto.Email}' already exists" });
                }

                // Create new user
                var user = new ApplicationUser
                {
                    UserName = createUserDto.UserName,
                    Email = createUserDto.Email,
                    Firstname = createUserDto.FirstName,
                    Lastname = createUserDto.LastName,
                    PhoneNumber = createUserDto.PhoneNumber,
                    IsActive = createUserDto.IsActive
                };

                // Create user with password
                var result = await _userManager.CreateAsync(user, createUserDto.Password);

                if (!result.Succeeded)
                {
                    return new ApiResponseDto<UserResponseDto>("Failed to create user", 
                        result.Errors.Select(e => e.Description).ToList());
                }

                // Get user with roles
                var createdUser = await _userManager.FindByNameAsync(user.UserName);
                var roles = await _userManager.GetRolesAsync(createdUser);

                var userResponse = MapToUserResponseDto(createdUser, roles.ToList());

                return new ApiResponseDto<UserResponseDto>(userResponse, "User created successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<UserResponseDto>("An error occurred while creating the user", 
                    new List<string> { ex.Message });
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        public async Task<ApiResponseDto<UserResponseDto>> GetUserByIdAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponseDto<UserResponseDto>("User not found", 
                        new List<string> { $"No user found with ID '{userId}'" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userResponse = MapToUserResponseDto(user, roles.ToList());

                return new ApiResponseDto<UserResponseDto>(userResponse, "User retrieved successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<UserResponseDto>("An error occurred while retrieving the user", 
                    new List<string> { ex.Message });
            }
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        public async Task<ApiResponseDto<UserResponseDto>> GetUserByUserNameAsync(string userName)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    return new ApiResponseDto<UserResponseDto>("User not found", 
                        new List<string> { $"No user found with username '{userName}'" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userResponse = MapToUserResponseDto(user, roles.ToList());

                return new ApiResponseDto<UserResponseDto>(userResponse, "User retrieved successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<UserResponseDto>("An error occurred while retrieving the user", 
                    new List<string> { ex.Message });
            }
        }

        /// <summary>
        /// Get all users with pagination and search
        /// </summary>
        public async Task<ApiResponseDto<PaginatedResultDto<UserResponseDto>>> GetAllUsersAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string searchTerm = null)
        {
            try
            {
                // Start with all users
                var query = _userManager.Users.AsQueryable();

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(u => 
                        u.UserName.Contains(searchTerm) ||
                        u.Email.Contains(searchTerm) ||
                        u.Firstname.Contains(searchTerm) ||
                        u.Lastname.Contains(searchTerm));
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination
                var users = await query
                    .OrderBy(u => u.UserName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Map to response DTOs with roles
                var userResponses = new List<UserResponseDto>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userResponses.Add(MapToUserResponseDto(user, roles.ToList()));
                }

                // Calculate pagination metadata
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var paginatedResult = new PaginatedResultDto<UserResponseDto>
                {
                    Items = userResponses,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasNextPage = pageNumber < totalPages,
                    HasPreviousPage = pageNumber > 1
                };

                return new ApiResponseDto<PaginatedResultDto<UserResponseDto>>(paginatedResult, 
                    $"Retrieved {userResponses.Count} users");
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<PaginatedResultDto<UserResponseDto>>(
                    "An error occurred while retrieving users", 
                    new List<string> { ex.Message });
            }
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        public async Task<ApiResponseDto<UserResponseDto>> UpdateUserAsync(string userId, UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponseDto<UserResponseDto>("User not found", 
                        new List<string> { $"No user found with ID '{userId}'" });
                }

                // Update user properties
                user.Firstname = updateUserDto.FirstName;
                user.Lastname = updateUserDto.LastName;
                user.Email = updateUserDto.Email;
                user.PhoneNumber = updateUserDto.PhoneNumber;
                user.IsActive = updateUserDto.IsActive;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return new ApiResponseDto<UserResponseDto>("Failed to update user", 
                        result.Errors.Select(e => e.Description).ToList());
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userResponse = MapToUserResponseDto(user, roles.ToList());

                return new ApiResponseDto<UserResponseDto>(userResponse, "User updated successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<UserResponseDto>("An error occurred while updating the user", 
                    new List<string> { ex.Message });
            }
        }

        /// <summary>
        /// Soft delete a user (set IsActive to false)
        /// </summary>
        public async Task<ApiResponseDto<bool>> DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponseDto<bool>("User not found", 
                        new List<string> { $"No user found with ID '{userId}'" });
                }

                // Soft delete by setting IsActive to false
                user.IsActive = false;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return new ApiResponseDto<bool>("Failed to delete user", 
                        result.Errors.Select(e => e.Description).ToList());
                }

                return new ApiResponseDto<bool>(true, "User deleted successfully (soft delete)");
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>("An error occurred while deleting the user", 
                    new List<string> { ex.Message });
            }
        }

        /// <summary>
        /// Permanently delete a user from the database
        /// </summary>
        public async Task<ApiResponseDto<bool>> PermanentlyDeleteUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponseDto<bool>("User not found", 
                        new List<string> { $"No user found with ID '{userId}'" });
                }

                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    return new ApiResponseDto<bool>("Failed to permanently delete user", 
                        result.Errors.Select(e => e.Description).ToList());
                }

                return new ApiResponseDto<bool>(true, "User permanently deleted");
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>("An error occurred while permanently deleting the user", 
                    new List<string> { ex.Message });
            }
        }

        /// <summary>
        /// Assign roles to a user
        /// </summary>
        public async Task<ApiResponseDto<bool>> AssignRolesToUserAsync(string userId, List<string> roles)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponseDto<bool>("User not found", 
                        new List<string> { $"No user found with ID '{userId}'" });
                }

                var result = await _userManager.AddToRolesAsync(user, roles);

                if (!result.Succeeded)
                {
                    return new ApiResponseDto<bool>("Failed to assign roles", 
                        result.Errors.Select(e => e.Description).ToList());
                }

                return new ApiResponseDto<bool>(true, $"Roles assigned successfully to user '{user.UserName}'");
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>("An error occurred while assigning roles", 
                    new List<string> { ex.Message });
            }
        }

        /// <summary>
        /// Remove roles from a user
        /// </summary>
        public async Task<ApiResponseDto<bool>> RemoveRolesFromUserAsync(string userId, List<string> roles)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponseDto<bool>("User not found", 
                        new List<string> { $"No user found with ID '{userId}'" });
                }

                var result = await _userManager.RemoveFromRolesAsync(user, roles);

                if (!result.Succeeded)
                {
                    return new ApiResponseDto<bool>("Failed to remove roles", 
                        result.Errors.Select(e => e.Description).ToList());
                }

                return new ApiResponseDto<bool>(true, $"Roles removed successfully from user '{user.UserName}'");
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>("An error occurred while removing roles", 
                    new List<string> { ex.Message });
            }
        }

        /// <summary>
        /// Get all roles for a user
        /// </summary>
        public async Task<ApiResponseDto<List<string>>> GetUserRolesAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponseDto<List<string>>("User not found", 
                        new List<string> { $"No user found with ID '{userId}'" });
                }

                var roles = await _userManager.GetRolesAsync(user);

                return new ApiResponseDto<List<string>>(roles.ToList(), 
                    $"Retrieved {roles.Count} roles for user '{user.UserName}'");
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<List<string>>("An error occurred while retrieving user roles", 
                    new List<string> { ex.Message });
            }
        }

        /// <summary>
        /// Helper method to map ApplicationUser to UserResponseDto
        /// </summary>
        private UserResponseDto MapToUserResponseDto(ApplicationUser user, List<string> roles)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.Firstname,
                LastName = user.Lastname,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                IsActive = user.IsActive,
                CreatedAt = DateTime.UtcNow, // Note: You may want to add a CreatedAt field to ApplicationUser
                Roles = roles
            };
        }
    }
}
