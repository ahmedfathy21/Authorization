using AuthSystemAPI.DTOs;

namespace AuthSystemAPI.Services
{
    /// <summary>
    /// Interface for authentication service
    /// </summary>
    public interface IAuthService
    {
        Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginDto loginDto);
        Task<ApiResponseDto<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto refreshTokenDto);
        Task<ApiResponseDto<bool>> LogoutAsync(LogoutDto logoutDto);
    }
}
