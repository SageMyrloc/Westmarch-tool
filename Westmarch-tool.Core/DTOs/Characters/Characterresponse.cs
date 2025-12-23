using Westmarch_tool.Core.Enums;

namespace Westmarch_tool.Core.DTOs.Characters
{
    public class CharacterResponse
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string? DualClass { get; set; }
        public int Level { get; set; }
        public int XP { get; set; }
        public string Ancestry { get; set; } = string.Empty;
        public string? Heritage { get; set; }
        public string Background { get; set; } = string.Empty;
        public string Alignment { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public string? Age { get; set; }
        public string? Deity { get; set; }
        public int Size { get; set; }
        public string SizeName { get; set; } = string.Empty;
        public string KeyAbility { get; set; } = string.Empty;
        public int FocusPoints { get; set; }
        public CharacterStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // Attributes
        public CharacterAttributesResponse? Attributes { get; set; }
    }

    public class CharacterAttributesResponse
    {
        public int AncestryHP { get; set; }
        public int ClassHP { get; set; }
        public int BonusHP { get; set; }
        public int BonusHPPerLevel { get; set; }
        public int Speed { get; set; }
        public int SpeedBonus { get; set; }

        // Calculated property
        public int TotalHP => AncestryHP + ClassHP + BonusHP;
    }
}