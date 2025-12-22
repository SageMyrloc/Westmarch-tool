using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Westmarch_tool.Core.DTOs.Auth.Requests;
using Westmarch_tool.Core.DTOs.Auth.Responses;
using Westmarch_tool.Core.Interfaces;
using Westmarch_tool.Infrastructure.Data;

namespace Westmarch_tool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly WestmarchDbContext _context;

        public AuthController(IAuthService authService, WestmarchDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username and password are required" });
            }

            var result = await _authService.RegisterAsync(request);

            if (result == null)
            {
                return BadRequest(new { message = "Username already exists" });
            }

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username and password are required" });
            }

            var result = await _authService.LoginAsync(request);

            if (result == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int playerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            // Load player with roles
            var player = await _context.Players
                .Where(p => p.Id == playerId)
                .Select(p => new MeResponse
                {
                    PlayerId = p.Id,
                    Username = p.Username,
                    DiscordId = p.DiscordId,
                    CreatedDate = p.CreatedDate,
                    LastLoginDate = p.LastLoginDate,
                    Roles = p.PlayerRoles.Select(pr => pr.Role.Name).ToList()
                })
                .FirstOrDefaultAsync();

            if (player == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(player);
        }
    }
}