namespace Westmarch_tool.Core.Entities.Characters
{
    public class CharacterAttributes
    {
        public int Id { get; set; }
        public int CharacterId { get; set; }

        // Ability Scores
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Constitution { get; set; }
        public int Intelligence { get; set; }
        public int Wisdom { get; set; }
        public int Charisma { get; set; }

        // HP
        public int AncestryHP { get; set; }
        public int ClassHP { get; set; }
        public int BonusHP { get; set; }
        public int BonusHPPerLevel { get; set; }

        // Speed
        public int Speed { get; set; }
        public int SpeedBonus { get; set; }

        // Navigation property
        public Character Character { get; set; } = null!;
    }
}