namespace Westmarch_tool.Core.Entities.Auth
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // "Admin", "DM", "Player"
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public ICollection<PlayerRole> PlayerRoles { get; set; } = new List<PlayerRole>();
    }
}