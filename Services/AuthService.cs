using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthSystemAPI.Data;
using AuthSystemAPI.DTOs;
using AuthSystemAPI.Entities;
using AuthSystemAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthSystemAPI.Services
{
    /// <summary>
    /// Service for authentication and token handling
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly int _accessTokenMinutes;
        private readonly int _refreshTokenDays;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _httpContextAccessor = httpContextAccessor;

            _jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing");
            _jwtIssuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is missing");
            _jwtAudience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is missing");
            _accessTokenMinutes = int.TryParse(configuration["Jwt:AccessTokenMinutes"], out var minutes) ? minutes : 30;
            _refreshTokenDays = int.TryParse(configuration["Jwt:RefreshTokenDays"], out var days) ? days : 7;
        }

        public async Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await FindByUserNameOrEmailAsync(loginDto.UserName);
                if (user == null)
                {
                    return new ApiResponseDto<AuthResponseDto>("Invalid credentials",
                        new List<string> { "Username or password is incorrect" });
                }

                var signInResult = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
                if (!signInResult.Succeeded)
                {
                    return new ApiResponseDto<AuthResponseDto>("Invalid credentials",
                        new List<string> { "Username or password is incorrect" });
                }

                var accessToken = await CreateJwtTokenAsync(user);
                var refreshToken = CreateRefreshToken();

                var refreshTokenEntity = new RefreshToken
                {
                    Token = refreshToken,
                    Expires = DateTime.UtcNow.AddDays(_refreshTokenDays),
                    Created = DateTime.UtcNow,
                    UserId = user.Id
                };

                var httpContext = _httpContextAccessor.HttpContext;
                var loginUserEntity = new LoginUser
                {
                    UserId = user.Id,
                    LoggedInAt = DateTime.UtcNow,
                    IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = httpContext?.Request.Headers.UserAgent.ToString()
                };

                _context.RefreshTokens.Add(refreshTokenEntity);
                _context.LoginUsers.Add(loginUserEntity);
                await _context.SaveChangesAsync();

                var response = new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_accessTokenMinutes)
                };

                return new ApiResponseDto<AuthResponseDto>(response, "Login successful");
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<AuthResponseDto>("An error occurred while logging in",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponseDto<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto refreshTokenDto)
        {
            try
            {
                var storedToken = await _context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == refreshTokenDto.RefreshToken);

                if (storedToken == null || !storedToken.IsActive)
                {
                    return new ApiResponseDto<AuthResponseDto>("Invalid refresh token",
                        new List<string> { "Refresh token is invalid or expired" });
                }

                var user = storedToken.User;
                var accessToken = await CreateJwtTokenAsync(user);
                var newRefreshToken = CreateRefreshToken();

                storedToken.Revoked = DateTime.UtcNow;
                storedToken.ReplacedByToken = newRefreshToken;

                var newTokenEntity = new RefreshToken
                {
                    Token = newRefreshToken,
                    Expires = DateTime.UtcNow.AddDays(_refreshTokenDays),
                    Created = DateTime.UtcNow,
                    UserId = user.Id
                };

                _context.RefreshTokens.Add(newTokenEntity);
                await _context.SaveChangesAsync();

                var response = new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_accessTokenMinutes)
                };

                return new ApiResponseDto<AuthResponseDto>(response, "Token refreshed successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<AuthResponseDto>("An error occurred while refreshing the token",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponseDto<bool>> LogoutAsync(LogoutDto logoutDto)
        {
            try
            {
                var storedToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == logoutDto.RefreshToken);

                if (storedToken == null || !storedToken.IsActive)
                {
                    return new ApiResponseDto<bool>("Refresh token not found",
                        new List<string> { "Refresh token is invalid or already revoked" });
                }

                storedToken.Revoked = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>(true, "Logout successful");
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>("An error occurred while logging out",
                    new List<string> { ex.Message });
            }
        }

        private async Task<ApplicationUser?> FindByUserNameOrEmailAsync(string userNameOrEmail)
        {
            if (userNameOrEmail.Contains('@'))
            {
                return await _userManager.FindByEmailAsync(userNameOrEmail);
            }

            return await _userManager.FindByNameAsync(userNameOrEmail);
        }

        private async Task<string> CreateJwtTokenAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_accessTokenMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string CreateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
