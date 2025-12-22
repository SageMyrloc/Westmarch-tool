namespace Westmarch_tool.Core.DTOs.Auth.Responses
{
    public class MeResponse
    {
        public int PlayerId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? DiscordId { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}