using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Westmarch_tool.Core.DTOs.Auth.Requests;
using Westmarch_tool.Core.DTOs.Auth.Responses;
using Westmarch_tool.Core.Entities.Auth;
using Westmarch_tool.Core.Interfaces;
using Westmarch_tool.Infrastructure.Data;

namespace Westmarch_tool.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly WestmarchDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(WestmarchDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            // Check if username already exists
            if (await _context.Players.AnyAsync(p => p.Username == request.Username))
            {
                return null; // Username taken
            }

            // Hash the password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create new player
            var player = new Player
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                DiscordId = request.DiscordId,
                CreatedDate = DateTime.UtcNow
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = GenerateJwtToken(player);

            return new AuthResponse
            {
                PlayerId = player.Id,
                Username = player.Username,
                Token = token,
                DiscordId = player.DiscordId
            };
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            // Find player by username
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.Username == request.Username);

            if (player == null)
            {
                return null; // User not found
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, player.PasswordHash))
            {
                return null; // Wrong password
            }

            // Update last login
            player.LastLoginDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = GenerateJwtToken(player);

            return new AuthResponse
            {
                PlayerId = player.Id,
                Username = player.Username,
                Token = token,
                DiscordId = player.DiscordId
            };
        }

        private string GenerateJwtToken(Player player)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
                new Claim(ClaimTypes.Name, player.Username),
                new Claim("DiscordId", player.DiscordId ?? string.Empty)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(_configuration["JwtSettings:ExpirationMinutes"]!)),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}