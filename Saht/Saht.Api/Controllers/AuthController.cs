using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Saht.Application.Abstractions;
using Saht.Application.Auth;

namespace Saht.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly ICurrentUser _current;
        private readonly IUserRepository _users;

        public AuthController(IAuthService auth, ICurrentUser current, IUserRepository users)
        {
            _auth = auth; _current = current; _users = users;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
        {
            var res = await _auth.RegisterAsync(
                new RegisterCommand(req.UserName, req.Email, req.DisplayName, req.Password),
                ct);

            Response.Cookies.Append("AuthToken", res.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(6)
            });

            return Ok(res);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
        {
            var res = await _auth.LoginAsync(new LoginCommand(req.UserNameOrEmail, req.Password), ct);

            Response.Cookies.Append("AuthToken", res.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(6)
            });

            return Ok(res); // body’de token’ı yine döndürüyoruz
        }


        public record RegisterRequest(string UserName, string Email, string DisplayName, string Password);
        public record LoginRequest(string UserNameOrEmail, string Password);

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me(CancellationToken ct)
        {
            if (!_current.IsAuthenticated || _current.UserId is null)
                return Unauthorized();

            var user = await _users.GetByIdAsync(_current.UserId.Value, ct);
            if (user is null) return NotFound();

            // Küçük bir view model
            return Ok(new
            {
                id = user.Id,
                userName = user.UserName,
                displayName = user.DisplayName,
                email = user.Email,
                role = user.Role.ToString(),
                bio = user.Bio,
                avatarUrl = user.AvatarUrl,
                createdAt = user.CreatedAt
            });
        }
    }
}
