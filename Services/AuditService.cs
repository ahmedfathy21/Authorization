using AuthSystemAPI.Data;
using AuthSystemAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AuthSystemAPI.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponseDto<PaginatedResultDto<LoginUserDto>>> GetLoginUsersAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? userId = null)
        {
            try
            {
                var query = _context.LoginUsers
                    .AsNoTracking()
                    .Include(lu => lu.User)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    query = query.Where(lu => lu.UserId == userId);
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(lu => lu.LoggedInAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(lu => new LoginUserDto
                    {
                        Id = lu.Id,
                        UserId = lu.UserId,
                        UserName = lu.User.UserName,
                        Email = lu.User.Email,
                        LoggedInAt = lu.LoggedInAt,
                        IpAddress = lu.IpAddress,
                        UserAgent = lu.UserAgent
                    })
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                var result = new PaginatedResultDto<LoginUserDto>
                {
                    Items = items,
                    TotalItems = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasNextPage = pageNumber < totalPages,
                    HasPreviousPage = pageNumber > 1
                };

                return new ApiResponseDto<PaginatedResultDto<LoginUserDto>>(result, "Login audit records retrieved");
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<PaginatedResultDto<LoginUserDto>>(
                    "An error occurred while retrieving login audit records",
                    new List<string> { ex.Message });
            }
        }
    }
}
