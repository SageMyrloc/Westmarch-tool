using Westmarch_tool.Core.Enums;

namespace Westmarch_tool.Core.DTOs.Characters.Responses
{
    public class CharacterListResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public int Level { get; set; }
        public string Ancestry { get; set; } = string.Empty;
        public CharacterStatus Status { get; set; }
    }
}