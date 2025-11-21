using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using Westmarch_tool.Core.DTOs.Characters.Requests;
using Westmarch_tool.Core.Interfaces;
using System.Text.Json;

namespace Westmarch_tool.API.Controllers.Characters
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CharactersController : ControllerBase
    {
        private readonly ICharacterService _characterService;

        public CharactersController(ICharacterService characterService)
        {
            _characterService = characterService;
        }

        private int GetPlayerId()
        {
            var playerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(playerIdClaim ?? "0");
        }

        [HttpGet]
        public async Task<IActionResult> GetMyCharacters()
        {
            var playerId = GetPlayerId();
            var characters = await _characterService.GetPlayerCharactersAsync(playerId);
            return Ok(characters);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCharacter(int id)
        {
            var playerId = GetPlayerId();
            var character = await _characterService.GetCharacterByIdAsync(id, playerId);

            if (character == null)
            {
                return NotFound(new { message = "Character not found" });
            }

            return Ok(character);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCharacter([FromBody] CreateCharacterRequest request)
        {
            var playerId = GetPlayerId();
            var character = await _characterService.CreateCharacterAsync(playerId, request);

            if (character == null)
            {
                return BadRequest(new { message = "Failed to create character" });
            }

            return CreatedAtAction(nameof(GetCharacter), new { id = character.Id }, character);
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportCharacter([FromBody] ImportCharacterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.JsonData))
            {
                return BadRequest(new { message = "JSON data is required" });
            }

            var playerId = GetPlayerId();
            var character = await _characterService.ImportCharacterAsync(playerId, request);

            if (character == null)
            {
                return BadRequest(new { message = "Failed to import character. Please check the JSON format." });
            }

            return CreatedAtAction(nameof(GetCharacter), new { id = character.Id }, character);
        }

        [HttpPost("import-raw")]
        public async Task<IActionResult> ImportCharacterRaw([FromBody] JsonElement pathbuilderJson)
        {
            try
            {
                // Convert the JsonElement to a string
                var jsonString = pathbuilderJson.GetRawText();

                var playerId = GetPlayerId();
                var character = await _characterService.ImportCharacterAsync(playerId, new ImportCharacterRequest
                {
                    JsonData = jsonString
                });

                if (character == null)
                {
                    return BadRequest(new { message = "Failed to import character. Please check the JSON format." });
                }

                return CreatedAtAction(nameof(GetCharacter), new { id = character.Id }, character);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error: {ex.Message}" });
            }
        }
    }
}