using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saht.Application.Abstractions;
using Saht.Application.Users;

namespace Saht.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class UsersController : ControllerBase
    {
        private readonly IUserService _users;
        private readonly ICurrentUser _current;

        public UsersController(IUserService users, ICurrentUser current)
        {
            _users = users; _current = current;
        }

        // 1) Public profile
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var dto = await _users.GetPublicAsync(id, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        // 2) Kendi profilini güncelle
        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req, CancellationToken ct)
        {
            if (_current.UserId is null) return Unauthorized();

            var dto = await _users.UpdateProfileAsync(
                _current.UserId.Value,
                new UpdateProfileCommand(req.DisplayName, req.Bio, req.AvatarUrl),
                ct);

            return Ok(dto);
        }

        // 3) Şifre değiştir
        [Authorize]
        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req, CancellationToken ct)
        {
            if (_current.UserId is null) return Unauthorized();

            await _users.ChangePasswordAsync(
                _current.UserId.Value,
                new ChangePasswordCommand(req.OldPassword, req.NewPassword),
                ct);

            return NoContent();
        }
        [HttpGet("by-username/{userName}")]
        public async Task<IActionResult> GetByUserName(string userName, CancellationToken ct)
        {
            var dto = await _users.GetPublicByUserNameAsync(userName, ct);
            return dto is null ? NotFound() : Ok(dto);
        }
   

        //// 3) Password Update  yaparız
        //[Authorize]
        //[HttpPut("password")]
        //public async Task<IActionResult> UpdatePassword([FromBody] ChangePasswordRequest req, CancellationToken ct)
        //{
        //    var me = _current.UserId ?? throw new UnauthorizedAccessException();
        //    await _users.ChangePasswordAsync(me, new ChangePasswordCommand(req.CurrentPassword, req.NewPassword), ct);
        //    return NoContent();
        //}

        public sealed record UpdateProfileRequest(string DisplayName, string? Bio, string? AvatarUrl);
        public sealed record ChangePasswordRequest(string OldPassword, string NewPassword);
    }
}
