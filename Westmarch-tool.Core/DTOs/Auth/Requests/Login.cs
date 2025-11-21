namespace Westmarch_tool.Core.DTOs.Auth.Requests

{
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}