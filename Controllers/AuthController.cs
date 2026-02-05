using System;
using AuthSystemAPI.DTOs;
using AuthSystemAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthSystemAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")] 
        public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
        {
            var response = await _authService.LoginAsync(loginDto);
            return ToActionResult(response);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> RefreshToken(
            [FromBody] RefreshTokenRequestDto refreshTokenDto)
        {
            var response = await _authService.RefreshTokenAsync(refreshTokenDto);
            return ToActionResult(response);
        }

        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponseDto<bool>>> Logout([FromBody] LogoutDto logoutDto)
        {
            var response = await _authService.LogoutAsync(logoutDto);
            return ToActionResult(response);
        }

        private ActionResult<ApiResponseDto<T>> ToActionResult<T>(ApiResponseDto<T> response)
        {
            if (response.Success)
            {
                return Ok(response);
            }

            if (!string.IsNullOrWhiteSpace(response.Message) &&
                response.Message.Contains("invalid credentials", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(response);
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
