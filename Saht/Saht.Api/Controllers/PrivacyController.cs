using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saht.Application.Abstractions;
using Saht.Application.SocialConnections;

namespace Saht.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class PrivacyController : ControllerBase
    {
        private readonly IPrivacyService _privacy;
        private readonly ICurrentUser _current;

        public PrivacyController(IPrivacyService privacy, ICurrentUser current)
        { _privacy = privacy; _current = current; }

        [HttpPost("block/{userId:guid}")]
        public async Task<IActionResult> Block(Guid userId, CancellationToken ct)
        {
            var me = _current.UserId ?? throw new UnauthorizedAccessException();
            await _privacy.BlockAsync(me, userId, ct);
            return NoContent();
        }

        [HttpDelete("block/{userId:guid}")]
        public async Task<IActionResult> Unblock(Guid userId, CancellationToken ct)
        {
            var me = _current.UserId ?? throw new UnauthorizedAccessException();
            await _privacy.UnblockAsync(me, userId, ct);
            return NoContent();
        }

        [HttpPost("mute/{userId:guid}")]
        public async Task<IActionResult> Mute(Guid userId, CancellationToken ct)
        {
            var me = _current.UserId ?? throw new UnauthorizedAccessException();
            await _privacy.MuteAsync(me, userId, ct);
            return NoContent();
        }

        [HttpDelete("mute/{userId:guid}")]
        public async Task<IActionResult> Unmute(Guid userId, CancellationToken ct)
        {
            var me = _current.UserId ?? throw new UnauthorizedAccessException();
            await _privacy.UnmuteAsync(me, userId, ct);
            return NoContent();
        }

        [HttpGet("muted")]
        public async Task<IActionResult> Muted(CancellationToken ct)
        {
            var me = _current.UserId ?? throw new UnauthorizedAccessException();
            var ids = await _privacy.GetMutedAsync(me, ct);
            return Ok(ids);
        }

        [HttpGet("blocked")]
        public async Task<IActionResult> Blocked(CancellationToken ct)
        {
            var me = _current.UserId ?? throw new UnauthorizedAccessException();
            var ids = await _privacy.GetBlockedAsync(me, ct);
            return Ok(ids);
        }
    }
}
