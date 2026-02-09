using AuthSystemAPI.DTOs;

namespace AuthSystemAPI.Services
{
    public interface IAuditService
    {
        Task<ApiResponseDto<PaginatedResultDto<LoginUserDto>>> GetLoginUsersAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? userId = null);
    }
}
