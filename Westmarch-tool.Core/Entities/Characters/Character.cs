using Westmarch_tool.Core.Entities.Auth;
using Westmarch_tool.Core.Enums;

namespace Westmarch_tool.Core.Entities.Characters
{
    public class Character
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
        public CharacterStatus Status { get; set; } = CharacterStatus.AwaitingAuthorisation;
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // Navigation properties
        public Player Player { get; set; } = null!;
        public CharacterAttributes? Attributes { get; set; }
    }
}