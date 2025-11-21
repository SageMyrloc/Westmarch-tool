namespace Westmarch_tool.Core.DTOs.Auth.Responses

{
    public class AuthResponse
    {
        public int PlayerId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string? DiscordId { get; set; }
    }
}