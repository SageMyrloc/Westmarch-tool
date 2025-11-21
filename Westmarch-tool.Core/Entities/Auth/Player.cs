using Westmarch_tool.Core.Entities.Characters;

namespace Westmarch_tool.Core.Entities.Auth
{
    public class Player
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? DiscordId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }

        // Navigation property
        public ICollection<Character> Characters { get; set; } = new List<Character>();
    }
}