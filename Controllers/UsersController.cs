using System;
using AuthSystemAPI.DTOs;
using AuthSystemAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthSystemAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<UserResponseDto>>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            var response = await _userService.CreateUserAsync(createUserDto);
            return ToActionResult(response);
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<ApiResponseDto<UserResponseDto>>> GetUserById(string userId)
        {
            var response = await _userService.GetUserByIdAsync(userId);
            return ToActionResult(response);
        }

        [HttpGet("by-username/{userName}")]
        public async Task<ActionResult<ApiResponseDto<UserResponseDto>>> GetUserByUserName(string userName)
        {
            var response = await _userService.GetUserByUserNameAsync(userName);
            return ToActionResult(response);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<PaginatedResultDto<UserResponseDto>>>> GetAllUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string searchTerm = null)
        {
            var response = await _userService.GetAllUsersAsync(pageNumber, pageSize, searchTerm);
            return ToActionResult(response);
        }

        [HttpPut("{userId}")]
        public async Task<ActionResult<ApiResponseDto<UserResponseDto>>> UpdateUser(
            string userId,
            [FromBody] UpdateUserDto updateUserDto)
        {
            var response = await _userService.UpdateUserAsync(userId, updateUserDto);
            return ToActionResult(response);
        }

        [HttpDelete("{userId}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteUser(string userId)
        {
            var response = await _userService.DeleteUserAsync(userId);
            return ToActionResult(response);
        }

        [HttpDelete("{userId}/permanent")]
        public async Task<ActionResult<ApiResponseDto<bool>>> PermanentlyDeleteUser(string userId)
        {
            var response = await _userService.PermanentlyDeleteUserAsync(userId);
            return ToActionResult(response);
        }

        [HttpPost("{userId}/roles")]
        public async Task<ActionResult<ApiResponseDto<bool>>> AssignRolesToUser(
            string userId,
            [FromBody] List<string> roles)
        {
            if (roles == null || roles.Count == 0)
            {
                return BadRequest(new ApiResponseDto<bool>(
                    "Roles list is required",
                    new List<string> { "roles must contain at least one item" }));
            }

            var response = await _userService.AssignRolesToUserAsync(userId, roles);
            return ToActionResult(response);
        }

        [HttpDelete("{userId}/roles")]
        public async Task<ActionResult<ApiResponseDto<bool>>> RemoveRolesFromUser(
            string userId,
            [FromBody] List<string> roles)
        {
            if (roles == null || roles.Count == 0)
            {
                return BadRequest(new ApiResponseDto<bool>(
                    "Roles list is required",
                    new List<string> { "roles must contain at least one item" }));
            }

            var response = await _userService.RemoveRolesFromUserAsync(userId, roles);
            return ToActionResult(response);
        }

        [HttpGet("{userId}/roles")]
        public async Task<ActionResult<ApiResponseDto<List<string>>>> GetUserRoles(string userId)
        {
            var response = await _userService.GetUserRolesAsync(userId);
            return ToActionResult(response);
        }

        private ActionResult<ApiResponseDto<T>> ToActionResult<T>(ApiResponseDto<T> response)
        {
            if (response.Success)
            {
                return Ok(response);
            }

            if (!string.IsNullOrWhiteSpace(response.Message) &&
                response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(response);
            }

            return BadRequest(response);
        }
    }
}
