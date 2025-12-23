using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Westmarch_tool.Core.Enums;
using Westmarch_tool.Infrastructure.Data;

namespace Westmarch_tool.API.Controllers.DM
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,DM")]
    public class DMController : ControllerBase
    {
        private readonly WestmarchDbContext _context;

        public DMController(WestmarchDbContext context)
        {
            _context = context;
        }

        // GET: api/dm/characters/pending
        [HttpGet("characters/pending")]
        public async Task<IActionResult> GetPendingCharacters()
        {
            var pendingCharacters = await _context.Characters
                .Include(c => c.Player)
                .Include(c => c.Attributes)
                .Where(c => c.Status == CharacterStatus.AwaitingAuthorisation)
                .OrderBy(c => c.CreatedDate)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Class,
                    c.DualClass,
                    c.Level,
                    c.Ancestry,
                    c.Heritage,
                    c.Background,
                    c.Alignment,
                    c.Gender,
                    c.Age,
                    c.Deity,
                    c.KeyAbility,
                    c.Size,
                    c.SizeName,
                    c.CreatedDate,
                    Player = new
                    {
                        c.Player.Id,
                        c.Player.Username,
                        c.Player.DiscordId
                    },
                    Attributes = c.Attributes == null ? null : new
                    {
                        c.Attributes.AncestryHP,
                        c.Attributes.ClassHP,
                        c.Attributes.Speed,
                        TotalHP = c.Attributes.AncestryHP + c.Attributes.ClassHP + c.Attributes.BonusHP
                    }
                })
                .ToListAsync();

            return Ok(pendingCharacters);
        }

        // POST: api/dm/characters/{characterId}/approve
        [HttpPost("characters/{characterId}/approve")]
        public async Task<IActionResult> ApproveCharacter(int characterId)
        {
            var character = await _context.Characters
                .Include(c => c.Player)
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (character == null)
            {
                return NotFound(new { message = "Character not found" });
            }

            if (character.Status != CharacterStatus.AwaitingAuthorisation)
            {
                return BadRequest(new { message = "Character is not pending approval" });
            }

            character.Status = CharacterStatus.Alive;
            character.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Character '{character.Name}' has been approved",
                characterId = character.Id,
                characterName = character.Name,
                playerUsername = character.Player.Username
            });
        }

        // POST: api/dm/characters/{characterId}/reject
        [HttpPost("characters/{characterId}/reject")]
        public async Task<IActionResult> RejectCharacter(int characterId, [FromBody] RejectCharacterRequest? request)
        {
            var character = await _context.Characters
                .Include(c => c.Player)
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (character == null)
            {
                return NotFound(new { message = "Character not found" });
            }

            if (character.Status != CharacterStatus.AwaitingAuthorisation)
            {
                return BadRequest(new { message = "Character is not pending approval" });
            }

            var characterName = character.Name;
            var playerUsername = character.Player.Username;

            // Delete rejected character
            _context.Characters.Remove(character);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Character '{characterName}' by {playerUsername} has been rejected and removed",
                reason = request?.Reason ?? "No reason provided"
            });
        }

        // GET: api/dm/characters/all
        [HttpGet("characters/all")]
        public async Task<IActionResult> GetAllCharacters()
        {
            var characters = await _context.Characters
                .Include(c => c.Player)
                .Include(c => c.Attributes)
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Class,
                    c.Level,
                    c.Ancestry,
                    c.Status,
                    c.XP,
                    c.CreatedDate,
                    c.LastModifiedDate,
                    Player = new
                    {
                        c.Player.Id,
                        c.Player.Username
                    },
                    Attributes = c.Attributes == null ? null : new
                    {
                        c.Attributes.AncestryHP,
                        c.Attributes.ClassHP,
                        TotalHP = c.Attributes.AncestryHP + c.Attributes.ClassHP + c.Attributes.BonusHP
                    }
                })
                .ToListAsync();

            return Ok(characters);
        }

        // PUT: api/dm/characters/{characterId}/status
        [HttpPut("characters/{characterId}/status")]
        public async Task<IActionResult> UpdateCharacterStatus(int characterId, [FromBody] UpdateCharacterStatusRequest request)
        {
            var character = await _context.Characters
                .Include(c => c.Player)
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (character == null)
            {
                return NotFound(new { message = "Character not found" });
            }

            character.Status = request.Status;
            character.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Character '{character.Name}' status updated to {request.Status}",
                characterId = character.Id,
                characterName = character.Name,
                newStatus = request.Status
            });
        }
    }

    // Request models
    public class RejectCharacterRequest
    {
        public string? Reason { get; set; }
    }

    public class UpdateCharacterStatusRequest
    {
        public CharacterStatus Status { get; set; }
    }
}