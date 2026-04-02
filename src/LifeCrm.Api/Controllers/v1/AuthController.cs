using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace LifeCrm.Api.Controllers.v1
{
    [AllowAnonymous]
    public class AuthController : ApiControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext context, IConfiguration config, ILogger<AuthController> logger)
        { _context = context; _config = config; _logger = logger; }

        [HttpPost("login")]
        [EnableRateLimiting("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            var user = await _context.Users
                .IgnoreQueryFilters()
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && !u.IsDeleted && u.IsActive, ct);

            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login for email: {Email}", request.Email);
                return Unauthorized(ApiResponse.Fail("Invalid email or password."));
            }

            user.LastLoginAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(ct);

            var token = GenerateJwt(user, user.Organization!);
            _logger.LogInformation("User {UserId} logged in.", user.Id);

            return OkResponse(new LoginResponse
            {
                Token = token.TokenString, ExpiresAt = token.ExpiresAt,
                UserId = user.Id.ToString(), FullName = user.FullName, Email = user.Email,
                Role = user.Role.ToString(), OrganizationId = user.OrganizationId.ToString(),
                OrganizationName = user.Organization!.Name
            });
        }

        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userId, out var id)) return Unauthorized();

            var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id, ct);
            if (user is null || !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                throw new ValidationException("CurrentPassword", "Current password is incorrect.");
            if (request.NewPassword.Length < 10)
                throw new ValidationException("NewPassword", "New password must be at least 10 characters.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.LastModifiedAt = DateTimeOffset.UtcNow;
            user.LastModifiedBy = user.Id.ToString();
            await _context.SaveChangesAsync(ct);
            return NoContentResponse();
        }

        private (string TokenString, DateTimeOffset ExpiresAt) GenerateJwt(
            Core.Entities.ApplicationUser user, Core.Entities.Organization org)
        {
            var jwtSettings  = _config.GetSection("Jwt");
            var secretKey    = jwtSettings["SecretKey"]!;
            var issuer       = jwtSettings["Issuer"]   ?? "LifeCrm";
            var audience     = jwtSettings["Audience"] ?? "LifeCrmWeb";
            var expiryMinutes = int.TryParse(jwtSettings["ExpiryMinutes"], out var mins) ? mins : 480;

            var key      = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds    = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
                new Claim("org_id",        user.OrganizationId.ToString()),
                new Claim("org_name",      org.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("name",          user.FullName)
            };

            var token = new JwtSecurityToken(issuer, audience, claims,
                expires: expiresAt.DateTime, signingCredentials: creds);
            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}
