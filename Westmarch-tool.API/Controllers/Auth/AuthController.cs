using Microsoft.AspNetCore.Mvc;
using Westmarch_tool.Core.DTOs.Auth.Requests;
using Westmarch_tool.Core.Interfaces;

namespace Westmarch_tool.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;

		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Username) ||
				string.IsNullOrWhiteSpace(request.Password))
			{
				return BadRequest(new { message = "Username and password are required" });
			}

			var result = await _authService.RegisterAsync(request);

			if (result == null)
			{
				return BadRequest(new { message = "Username already exists" });
			}

			return Ok(result);
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Username) ||
				string.IsNullOrWhiteSpace(request.Password))
			{
				return BadRequest(new { message = "Username and password are required" });
			}

			var result = await _authService.LoginAsync(request);

			if (result == null)
			{
				return Unauthorized(new { message = "Invalid username or password" });
			}

			return Ok(result);
		}
	}
}