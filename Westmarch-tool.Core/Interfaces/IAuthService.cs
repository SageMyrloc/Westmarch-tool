using Westmarch_tool.Core.DTOs.Auth.Requests;
using Westmarch_tool.Core.DTOs.Auth.Responses;

namespace Westmarch_tool.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse?> RegisterAsync(RegisterRequest request);
        Task<AuthResponse?> LoginAsync(LoginRequest request);
    }
}