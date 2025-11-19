using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Saht.Application.Abstractions;
using Saht.Domain.Users;

namespace Saht.Infrastructure.Security
{
    public sealed class JwtProvider : IJwtProvider
    {
        private readonly SymmetricSecurityKey _key;
        private readonly string _issuer, _audience;
        private readonly int _minutes;

        public JwtProvider(IConfiguration cfg)
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Key"]!));
            _issuer = cfg["Jwt:Issuer"]!;
            _audience = cfg["Jwt:Audience"]!;
            _minutes = int.TryParse(cfg["Jwt:AccessTokenMinutes"], out var m) ? m : 360;
        }

        public string Create(User user)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_issuer, _audience, claims,
                expires: DateTime.UtcNow.AddMinutes(_minutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}