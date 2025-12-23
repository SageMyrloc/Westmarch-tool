namespace Westmarch_tool.Core.DTOs.Characters
{
    public class CharacterCreateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string? DualClass { get; set; }
        public int Level { get; set; }
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

        // Attributes
        public int AncestryHP { get; set; }
        public int ClassHP { get; set; }
        public int Speed { get; set; }
    }
}