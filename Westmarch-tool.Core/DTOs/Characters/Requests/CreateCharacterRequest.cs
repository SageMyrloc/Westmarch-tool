namespace Westmarch_tool.Core.DTOs.Characters.Requests
{
    public class CreateCharacterRequest
    {
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

        // Ability Scores
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Constitution { get; set; }
        public int Intelligence { get; set; }
        public int Wisdom { get; set; }
        public int Charisma { get; set; }

        // HP & Speed
        public int AncestryHP { get; set; }
        public int ClassHP { get; set; }
        public int BonusHP { get; set; }
        public int BonusHPPerLevel { get; set; }
        public int Speed { get; set; }
        public int SpeedBonus { get; set; }
    }
}