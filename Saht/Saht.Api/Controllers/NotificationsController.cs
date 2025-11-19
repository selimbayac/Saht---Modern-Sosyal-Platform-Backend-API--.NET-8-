using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saht.Application.Abstractions;
using Saht.Application.Notifications;
using Saht.Domain.Posts;

namespace Saht.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // tüm bildirim uçları auth ister
    public sealed class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notifications;
        private readonly ICurrentUser _current;

        public NotificationsController(INotificationService notifications, ICurrentUser current)
        {
            _notifications = notifications;
            _current = current;
        }

        /// <summary>
        /// Bildirim listesi (sayfalı).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            var paged = await _notifications.ListAsync(uid, page, size, ct);
            return Ok(paged);
        }

        /// <summary>
        /// Okunmamış bildirim sayısı.
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount(CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            var count = await _notifications.UnreadCountAsync(uid, ct);
            return Ok(new UnreadCountDto(count));
        }

        /// <summary>
        /// Tek bildirimi okundu işaretle.
        /// </summary>
        [HttpPost("{id:long}/read")]
        public async Task<IActionResult> MarkRead(long id, CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            await _notifications.MarkReadAsync(uid, id, ct);
            return NoContent();
        }

        /// <summary>
        /// Tüm bildirimleri okundu işaretle.
        /// </summary>
        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllRead(CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            await _notifications.MarkAllReadAsync(uid, ct);
            return NoContent();
        }
       
    
        // --- DTOs (controller içi) ---

        public sealed record UnreadCountDto(int Count);
    }
}
