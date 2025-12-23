using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Westmarch_tool.Core.Entities.Auth;
using Westmarch_tool.Infrastructure.Data;

namespace Westmarch_tool.API.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly WestmarchDbContext _context;

        public AdminController(WestmarchDbContext context)
        {
            _context = context;
        }

        // GET: api/admin/users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Players
                .Include(p => p.PlayerRoles)
                    .ThenInclude(pr => pr.Role)
                .Select(p => new
                {
                    p.Id,
                    p.Username,
                    p.DiscordId,
                    p.CreatedDate,
                    p.LastLoginDate,
                    Roles = p.PlayerRoles.Select(pr => new
                    {
                        pr.RoleId,
                        pr.Role.Name,
                        pr.AssignedDate
                    }).ToList()
                })
                .OrderBy(p => p.Username)
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/admin/roles
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _context.Roles
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Description
                })
                .ToListAsync();

            return Ok(roles);
        }

        // POST: api/admin/users/{userId}/roles/{roleId}
        [HttpPost("users/{userId}/roles/{roleId}")]
        public async Task<IActionResult> AssignRole(int userId, int roleId)
        {
            // Check if user exists
            var user = await _context.Players.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Check if role exists
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            // Check if user already has this role
            var existingAssignment = await _context.PlayerRoles
                .FirstOrDefaultAsync(pr => pr.PlayerId == userId && pr.RoleId == roleId);

            if (existingAssignment != null)
            {
                return BadRequest(new { message = "User already has this role" });
            }

            // Assign role
            var playerRole = new PlayerRole
            {
                PlayerId = userId,
                RoleId = roleId,
                AssignedDate = DateTime.UtcNow
            };

            _context.PlayerRoles.Add(playerRole);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Role '{role.Name}' assigned to user '{user.Username}'",
                roleId = roleId,
                roleName = role.Name,
                assignedDate = playerRole.AssignedDate
            });
        }

        // DELETE: api/admin/users/{userId}/roles/{roleId}
        [HttpDelete("users/{userId}/roles/{roleId}")]
        public async Task<IActionResult> RemoveRole(int userId, int roleId)
        {
            var playerRole = await _context.PlayerRoles
                .Include(pr => pr.Role)
                .Include(pr => pr.Player)
                .FirstOrDefaultAsync(pr => pr.PlayerId == userId && pr.RoleId == roleId);

            if (playerRole == null)
            {
                return NotFound(new { message = "Role assignment not found" });
            }

            // Prevent removing the last "Player" role - everyone should have at least Player role
            if (playerRole.Role.Name == "Player")
            {
                var playerRoleCount = await _context.PlayerRoles
                    .Where(pr => pr.PlayerId == userId)
                    .CountAsync();

                if (playerRoleCount == 1)
                {
                    return BadRequest(new { message = "Cannot remove the last role from a user. Users must have at least the 'Player' role." });
                }
            }

            _context.PlayerRoles.Remove(playerRole);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Role '{playerRole.Role.Name}' removed from user '{playerRole.Player.Username}'"
            });
        }

        // GET: api/admin/users/{userId}
        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUser(int userId)
        {
            var user = await _context.Players
                .Include(p => p.PlayerRoles)
                    .ThenInclude(pr => pr.Role)
                .Include(p => p.Characters)
                .Where(p => p.Id == userId)
                .Select(p => new
                {
                    p.Id,
                    p.Username,
                    p.DiscordId,
                    p.CreatedDate,
                    p.LastLoginDate,
                    Roles = p.PlayerRoles.Select(pr => new
                    {
                        pr.RoleId,
                        pr.Role.Name,
                        pr.Role.Description,
                        pr.AssignedDate
                    }).ToList(),
                    Characters = p.Characters.Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Class,
                        c.Level,
                        c.Status
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }
    }
}