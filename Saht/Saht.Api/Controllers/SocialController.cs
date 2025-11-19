using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saht.Application.Abstractions;
using Saht.Application.SocialConnections;

namespace Saht.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class SocialController : ControllerBase
    {
        private readonly IFollowService _follows;
        private readonly ICurrentUser _current;
        public SocialController(IFollowService follows, ICurrentUser current)
        { _follows = follows; _current = current; }

        [Authorize]
        [HttpPost("follow/{userId:guid}")]
        public async Task<IActionResult> Follow(Guid userId, CancellationToken ct)
        {
            var me = _current.UserId ?? throw new UnauthorizedAccessException();
            await _follows.FollowAsync(me, userId, ct);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("follow/{userId:guid}")]
        public async Task<IActionResult> Unfollow(Guid userId, CancellationToken ct)
        {
            var me = _current.UserId ?? throw new UnauthorizedAccessException();
            await _follows.UnfollowAsync(me, userId, ct);
            return NoContent();
        }

        [HttpGet("counts/{userId:guid}")]
        public async Task<IActionResult> Counts(Guid userId, CancellationToken ct)
        {
            var (followers, following) = await _follows.CountsAsync(userId, ct);
            return Ok(new { followers, following });
        }

        [Authorize]
        [HttpGet("following")]
        public async Task<IActionResult> MyFollowing([FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
        {
            var me = _current.UserId ?? throw new UnauthorizedAccessException();
            var ids = await _follows.GetFollowingIdsAsync(me, page, size, ct);
            return Ok(ids);
        }

        [HttpGet("followers/{userId:guid}")]
        public async Task<IActionResult> Followers(Guid userId, [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
        {
            var (items, total) = await _follows.GetFollowersAsync(userId, page, size, ct);
            return Ok(new { total, items });
        }
        [HttpGet("following/{userId:guid}")]
        public async Task<IActionResult> Following(Guid userId, [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
        {
            var (items, total) = await _follows.GetFollowingAsync(userId, page, size, ct);
            return Ok(new { total, items });
        }
        [Authorize]
        [HttpGet("is-following/{target:guid}")]
        public async Task<IActionResult> IsFollowing(Guid target, CancellationToken ct)
        {
            var me = _current.UserId ?? throw new UnauthorizedAccessException();
            var ok = await _follows.IsFollowingAsync(me, target, ct);
            return Ok(new { isFollowing = ok });
        }
    }
}
