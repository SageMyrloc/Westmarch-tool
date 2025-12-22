namespace Westmarch_tool.Core.Entities.Auth
{
    public class PlayerRole
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int RoleId { get; set; }
        public DateTime AssignedDate { get; set; }

        // Navigation properties
        public Player Player { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }
}