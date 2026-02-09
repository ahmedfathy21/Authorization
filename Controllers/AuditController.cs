using AuthSystemAPI.DTOs;
using AuthSystemAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthSystemAPI.Controllers
{
    [ApiController]
    [Route("api/audit")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet("login-users")]
        public async Task<ActionResult<ApiResponseDto<PaginatedResultDto<LoginUserDto>>>> GetLoginUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? userId = null)
        {
            var response = await _auditService.GetLoginUsersAsync(pageNumber, pageSize, userId);
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
