using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Westmarch_tool.Core.DTOs.Characters.Requests;
using Westmarch_tool.Core.DTOs.Characters.Responses;
using Westmarch_tool.Core.Entities.Characters;
using Westmarch_tool.Core.Enums;
using Westmarch_tool.Core.Interfaces;
using Westmarch_tool.Infrastructure.Data;

namespace Westmarch_tool.Infrastructure.Services
{
    public class CharacterService : ICharacterService
    {
        private readonly WestmarchDbContext _context;
        private readonly ILogger<CharacterService> _logger;

        public CharacterService(WestmarchDbContext context, ILogger<CharacterService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CharacterResponse?> CreateCharacterAsync(int playerId, CreateCharacterRequest request)
        {
            var character = new Character
            {
                PlayerId = playerId,
                Name = request.Name,
                Class = request.Class,
                DualClass = request.DualClass,
                Level = request.Level,
                XP = request.XP,
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
                FocusPoints = request.FocusPoints,
                Status = CharacterStatus.AwaitingAuthorisation,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };

            var attributes = new CharacterAttributes
            {
                Character = character,
                Strength = request.Strength,
                Dexterity = request.Dexterity,
                Constitution = request.Constitution,
                Intelligence = request.Intelligence,
                Wisdom = request.Wisdom,
                Charisma = request.Charisma,
                AncestryHP = request.AncestryHP,
                ClassHP = request.ClassHP,
                BonusHP = request.BonusHP,
                BonusHPPerLevel = request.BonusHPPerLevel,
                Speed = request.Speed,
                SpeedBonus = request.SpeedBonus
            };

            _context.Characters.Add(character);
            _context.CharacterAttributes.Add(attributes);
            await _context.SaveChangesAsync();

            return await GetCharacterByIdAsync(character.Id, playerId);
        }

        public async Task<CharacterResponse?> ImportCharacterAsync(int playerId, ImportCharacterRequest request)
        {
            try
            {
                _logger.LogInformation("Starting character import for player {PlayerId}", playerId);
                _logger.LogInformation("JSON Data length: {Length}", request.JsonData?.Length ?? 0);

                var jsonDoc = JsonDocument.Parse(request.JsonData);
                var root = jsonDoc.RootElement;

                _logger.LogInformation("JSON parsed successfully");

                if (!root.TryGetProperty("build", out var build))
                {
                    _logger.LogError("JSON does not contain 'build' property");
                    return null;
                }

                _logger.LogInformation("Found build property, extracting character data");

                var name = build.GetProperty("name").GetString();
                _logger.LogInformation("Character name: {Name}", name);

                var character = new Character
                {
                    PlayerId = playerId,
                    Name = name ?? "Unknown",
                    Class = build.GetProperty("class").GetString() ?? "Unknown",
                    DualClass = build.TryGetProperty("dualClass", out var dualClass) && dualClass.ValueKind != JsonValueKind.Null
                        ? dualClass.GetString()
                        : null,
                    Level = build.GetProperty("level").GetInt32(),
                    XP = build.GetProperty("xp").GetInt32(),
                    Ancestry = build.GetProperty("ancestry").GetString() ?? "Unknown",
                    Heritage = build.GetProperty("heritage").GetString(),
                    Background = build.GetProperty("background").GetString() ?? "Unknown",
                    Alignment = build.GetProperty("alignment").GetString() ?? "N",
                    Gender = build.GetProperty("gender").GetString(),
                    Age = build.GetProperty("age").GetString(),
                    Deity = build.GetProperty("deity").GetString(),
                    Size = build.GetProperty("size").GetInt32(),
                    SizeName = build.GetProperty("sizeName").GetString() ?? "Medium",
                    KeyAbility = build.GetProperty("keyability").GetString() ?? "str",
                    FocusPoints = build.GetProperty("focusPoints").GetInt32(),
                    Status = CharacterStatus.AwaitingAuthorisation,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };

                _logger.LogInformation("Character object created: {Name}, Level {Level}", character.Name, character.Level);

                var attributesData = build.GetProperty("attributes");
                var abilitiesData = build.GetProperty("abilities");

                var attributes = new CharacterAttributes
                {
                    Character = character,
                    Strength = abilitiesData.GetProperty("str").GetInt32(),
                    Dexterity = abilitiesData.GetProperty("dex").GetInt32(),
                    Constitution = abilitiesData.GetProperty("con").GetInt32(),
                    Intelligence = abilitiesData.GetProperty("int").GetInt32(),
                    Wisdom = abilitiesData.GetProperty("wis").GetInt32(),
                    Charisma = abilitiesData.GetProperty("cha").GetInt32(),
                    AncestryHP = attributesData.GetProperty("ancestryhp").GetInt32(),
                    ClassHP = attributesData.GetProperty("classhp").GetInt32(),
                    BonusHP = attributesData.GetProperty("bonushp").GetInt32(),
                    BonusHPPerLevel = attributesData.GetProperty("bonushpPerLevel").GetInt32(),
                    Speed = attributesData.GetProperty("speed").GetInt32(),
                    SpeedBonus = attributesData.GetProperty("speedBonus").GetInt32()
                };

                _logger.LogInformation("Attributes created: STR {Str}, INT {Int}", attributes.Strength, attributes.Intelligence);

                _context.Characters.Add(character);
                _context.CharacterAttributes.Add(attributes);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Character saved successfully with ID {Id}", character.Id);

                return await GetCharacterByIdAsync(character.Id, playerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing character: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<IEnumerable<CharacterListResponse>> GetPlayerCharactersAsync(int playerId)
        {
            return await _context.Characters
                .Where(c => c.PlayerId == playerId)
                .Select(c => new CharacterListResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Class = c.Class,
                    Level = c.Level,
                    Ancestry = c.Ancestry,
                    Status = c.Status
                })
                .ToListAsync();
        }

        public async Task<CharacterResponse?> GetCharacterByIdAsync(int characterId, int playerId)
        {
            var character = await _context.Characters
                .Include(c => c.Attributes)
                .FirstOrDefaultAsync(c => c.Id == characterId && c.PlayerId == playerId);

            if (character == null)
                return null;

            return new CharacterResponse
            {
                Id = character.Id,
                Name = character.Name,
                Class = character.Class,
                DualClass = character.DualClass,
                Level = character.Level,
                XP = character.XP,
                Ancestry = character.Ancestry,
                Heritage = character.Heritage,
                Background = character.Background,
                Alignment = character.Alignment,
                Gender = character.Gender,
                Age = character.Age,
                Deity = character.Deity,
                Size = character.Size,
                SizeName = character.SizeName,
                KeyAbility = character.KeyAbility,
                FocusPoints = character.FocusPoints,
                Status = character.Status,
                CreatedDate = character.CreatedDate,
                LastModifiedDate = character.LastModifiedDate,
                Strength = character.Attributes?.Strength ?? 10,
                Dexterity = character.Attributes?.Dexterity ?? 10,
                Constitution = character.Attributes?.Constitution ?? 10,
                Intelligence = character.Attributes?.Intelligence ?? 10,
                Wisdom = character.Attributes?.Wisdom ?? 10,
                Charisma = character.Attributes?.Charisma ?? 10,
                AncestryHP = character.Attributes?.AncestryHP ?? 0,
                ClassHP = character.Attributes?.ClassHP ?? 0,
                BonusHP = character.Attributes?.BonusHP ?? 0,
                BonusHPPerLevel = character.Attributes?.BonusHPPerLevel ?? 0,
                Speed = character.Attributes?.Speed ?? 0,
                SpeedBonus = character.Attributes?.SpeedBonus ?? 0
            };
        }
    }
}