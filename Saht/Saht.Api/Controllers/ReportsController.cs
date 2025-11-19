using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saht.Api.Requests;
using Saht.Application.Abstractions;
using Saht.Application.Reports;
using Saht.Domain.Common;
using Saht.Domain.Reports;

namespace Saht.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class ReportsController : ControllerBase
    {
        private readonly IReportService _reports;
        private readonly ICurrentUser _current;

        public ReportsController(IReportService reports, ICurrentUser current)
        { _reports = reports; _current = current; }

        // Kullanıcı: rapor oluştur
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReportCreateReq req, CancellationToken ct)
        {
            var me = _current.UserId ?? throw new UnauthorizedAccessException();
            var id = await _reports.CreateAsync(me, req.TargetType, req.TargetId, req.Reason, ct);
            return Ok(new { id });
        }

        // Mod: liste
        [Authorize(Roles = "Moderator,Kurucu")]
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] ReportStatus? status, [FromQuery] ContentType? type, int page = 1, int size = 20, CancellationToken ct = default)
        {
            var (items, total) = await _reports.ListAsync(status, type, page, size, ct);
            return Ok(new { total, items });
        }

        // Mod: kararlar
        [Authorize(Roles = "Moderator,Kurucu")]
        [HttpPost("{id:guid}/review")]
        public async Task<IActionResult> Review(Guid id, [FromBody] NoteReq req, CancellationToken ct)
        {
            var _ = _current.UserId ?? throw new UnauthorizedAccessException();
            await _reports.ReviewAsync(_, id, req.Note, ct);
            return NoContent();
        }

        [Authorize(Roles = "Moderator,Kurucu")]
        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] NoteReq req, CancellationToken ct)
        {
            var _ = _current.UserId ?? throw new UnauthorizedAccessException();
            await _reports.RejectAsync(_, id, req.Note, ct);
            return NoContent();
        }

        [Authorize(Roles = "Moderator,Kurucu")]
        [HttpPost("{id:guid}/action")]
        public async Task<IActionResult> Action(Guid id, [FromBody] NoteReq req, CancellationToken ct)
        {
            var _ = _current.UserId ?? throw new UnauthorizedAccessException();
            await _reports.TakeActionAsync(_, id, req.Note, ct);
            return NoContent();
        }

        // Kullanıcı: kendi raporları
        [Authorize]
        [HttpGet("mine")]
        public async Task<IActionResult> Mine([FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
        {
            var me = _current.UserId ?? throw new UnauthorizedAccessException();
            var (items, total) = await _reports.MyReportsAsync(me, page, size, ct);
            return Ok(new { total, items });
        }

        // ---- DTOs ----
      
    }
}
