using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Westmarch_tool.Core.DTOs.Characters;
using Westmarch_tool.Core.Entities.Characters;
using Westmarch_tool.Core.Enums;
using Westmarch_tool.Infrastructure.Data;

namespace Westmarch_tool.API.Controllers.Characters
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CharactersController : ControllerBase
    {
        private readonly WestmarchDbContext _context;

        public CharactersController(WestmarchDbContext context)
        {
            _context = context;
        }

        // GET: api/characters/my
        [HttpGet("my")]
        public async Task<IActionResult> GetMyCharacters()
        {
            var playerId = GetCurrentPlayerId();
            if (playerId == null)
            {
                return Unauthorized();
            }

            var characters = await _context.Characters
                .Include(c => c.Attributes)
                .Where(c => c.PlayerId == playerId)
                .Select(c => new CharacterResponse
                {
                    Id = c.Id,
                    PlayerId = c.PlayerId,
                    Name = c.Name,
                    Class = c.Class,
                    DualClass = c.DualClass,
                    Level = c.Level,
                    XP = c.XP,
                    Ancestry = c.Ancestry,
                    Heritage = c.Heritage,
                    Background = c.Background,
                    Alignment = c.Alignment,
                    Gender = c.Gender,
                    Age = c.Age,
                    Deity = c.Deity,
                    Size = c.Size,
                    SizeName = c.SizeName,
                    KeyAbility = c.KeyAbility,
                    FocusPoints = c.FocusPoints,
                    Status = c.Status,
                    CreatedDate = c.CreatedDate,
                    LastModifiedDate = c.LastModifiedDate,
                    Attributes = c.Attributes == null ? null : new CharacterAttributesResponse
                    {
                        AncestryHP = c.Attributes.AncestryHP,
                        ClassHP = c.Attributes.ClassHP,
                        BonusHP = c.Attributes.BonusHP,
                        BonusHPPerLevel = c.Attributes.BonusHPPerLevel,
                        Speed = c.Attributes.Speed,
                        SpeedBonus = c.Attributes.SpeedBonus
                    }
                })
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            return Ok(characters);
        }

        // GET: api/characters/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCharacter(int id)
        {
            var playerId = GetCurrentPlayerId();
            if (playerId == null)
            {
                return Unauthorized();
            }

            var character = await _context.Characters
                .Include(c => c.Attributes)
                .Where(c => c.Id == id)
                .Select(c => new CharacterResponse
                {
                    Id = c.Id,
                    PlayerId = c.PlayerId,
                    Name = c.Name,
                    Class = c.Class,
                    DualClass = c.DualClass,
                    Level = c.Level,
                    XP = c.XP,
                    Ancestry = c.Ancestry,
                    Heritage = c.Heritage,
                    Background = c.Background,
                    Alignment = c.Alignment,
                    Gender = c.Gender,
                    Age = c.Age,
                    Deity = c.Deity,
                    Size = c.Size,
                    SizeName = c.SizeName,
                    KeyAbility = c.KeyAbility,
                    FocusPoints = c.FocusPoints,
                    Status = c.Status,
                    CreatedDate = c.CreatedDate,
                    LastModifiedDate = c.LastModifiedDate,
                    Attributes = c.Attributes == null ? null : new CharacterAttributesResponse
                    {
                        AncestryHP = c.Attributes.AncestryHP,
                        ClassHP = c.Attributes.ClassHP,
                        BonusHP = c.Attributes.BonusHP,
                        BonusHPPerLevel = c.Attributes.BonusHPPerLevel,
                        Speed = c.Attributes.Speed,
                        SpeedBonus = c.Attributes.SpeedBonus
                    }
                })
                .FirstOrDefaultAsync();

            if (character == null)
            {
                return NotFound(new { message = "Character not found" });
            }

            // Only owner or admin/DM can view
            var isOwner = character.PlayerId == playerId;
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("DM");

            if (!isOwner && !isAdmin)
            {
                return Forbid();
            }

            return Ok(character);
        }

        // POST: api/characters
        [HttpPost]
        public async Task<IActionResult> CreateCharacter([FromBody] CharacterCreateRequest request)
        {
            var playerId = GetCurrentPlayerId();
            if (playerId == null)
            {
                return Unauthorized();
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Class) ||
                string.IsNullOrWhiteSpace(request.Ancestry))
            {
                return BadRequest(new { message = "Name, Class, and Ancestry are required" });
            }

            var character = new Character
            {
                PlayerId = playerId.Value,
                Name = request.Name,
                Class = request.Class,
                DualClass = request.DualClass,
                Level = request.Level > 0 ? request.Level : 1,
                XP = 0,
                Ancestry = request.Ancestry,
                Heritage = request.Heritage,
                Background = request.Background,
                Alignment = request.Alignment,
                Gender = request.Gender,
                Age = request.Age,
                Deity = request.Deity,
                Size = request.Size,
                SizeName = request.SizeName,
                KeyAbility = request.KeyAbility,
                FocusPoints = 0,
                Status = CharacterStatus.AwaitingAuthorisation,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };

            _context.Characters.Add(character);
            await _context.SaveChangesAsync();

            // Create attributes
            var attributes = new CharacterAttributes
            {
                CharacterId = character.Id,
                AncestryHP = request.AncestryHP,
                ClassHP = request.ClassHP,
                BonusHP = 0,
                BonusHPPerLevel = 0,
                Speed = request.Speed,
                SpeedBonus = 0
            };

            _context.CharacterAttributes.Add(attributes);
            await _context.SaveChangesAsync();

            // Load the complete character for response
            var createdCharacter = await _context.Characters
                .Include(c => c.Attributes)
                .Where(c => c.Id == character.Id)
                .Select(c => new CharacterResponse
                {
                    Id = c.Id,
                    PlayerId = c.PlayerId,
                    Name = c.Name,
                    Class = c.Class,
                    DualClass = c.DualClass,
                    Level = c.Level,
                    XP = c.XP,
                    Ancestry = c.Ancestry,
                    Heritage = c.Heritage,
                    Background = c.Background,
                    Alignment = c.Alignment,
                    Gender = c.Gender,
                    Age = c.Age,
                    Deity = c.Deity,
                    Size = c.Size,
                    SizeName = c.SizeName,
                    KeyAbility = c.KeyAbility,
                    FocusPoints = c.FocusPoints,
                    Status = c.Status,
                    CreatedDate = c.CreatedDate,
                    LastModifiedDate = c.LastModifiedDate,
                    Attributes = c.Attributes == null ? null : new CharacterAttributesResponse
                    {
                        AncestryHP = c.Attributes.AncestryHP,
                        ClassHP = c.Attributes.ClassHP,
                        BonusHP = c.Attributes.BonusHP,
                        BonusHPPerLevel = c.Attributes.BonusHPPerLevel,
                        Speed = c.Attributes.Speed,
                        SpeedBonus = c.Attributes.SpeedBonus
                    }
                })
                .FirstAsync();

            return CreatedAtAction(nameof(GetCharacter), new { id = character.Id }, createdCharacter);
        }

        // DELETE: api/characters/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCharacter(int id)
        {
            var playerId = GetCurrentPlayerId();
            if (playerId == null)
            {
                return Unauthorized();
            }

            var character = await _context.Characters.FindAsync(id);

            if (character == null)
            {
                return NotFound(new { message = "Character not found" });
            }

            // Only owner can delete, and only if not yet approved
            if (character.PlayerId != playerId)
            {
                return Forbid();
            }

            if (character.Status != CharacterStatus.AwaitingAuthorisation)
            {
                return BadRequest(new { message = "Cannot delete a character that has been approved or is in active play" });
            }

            _context.Characters.Remove(character);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Character deleted successfully" });
        }

        private int? GetCurrentPlayerId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int playerId))
            {
                return null;
            }
            return playerId;
        }
    }
}