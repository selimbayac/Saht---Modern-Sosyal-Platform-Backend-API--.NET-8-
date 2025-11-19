using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saht.Api.Requests;
using Saht.Application.Abstractions;
using Saht.Application.Comments;
using Saht.Application.Reactions;

namespace Saht.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : Controller
    {
        private readonly IReactionService _reactions;
        private readonly ICurrentUser _current;
        private readonly ICommentService _comments;
        public CommentsController(IReactionService reactions, ICurrentUser current,ICommentService commentService)
        {
            _reactions = reactions;
            _current = current;
            _comments = commentService;
        }

        // ... (yorum create/reply/edit/delete vs. senin mevcut aksiyonların)

        // 1) Yorum için summary
        [HttpGet("{id:long}/reactions/summary")]
        public async Task<IActionResult> Summary(long id, CancellationToken ct)
        {
            var viewer = _current.UserId ?? Guid.Empty;
            var dto = await _reactions.GetCommentSummaryAsync(viewer, id, ct);
            return Ok(dto);
        }

        // 2) Kimler like/dislike etmiş (opsiyonel ?value=1|-1)
        [HttpGet("{id:long}/reactions")]
        public async Task<IActionResult> Reactors(long id, [FromQuery] int? value, [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
        {
            var (items, total) = await _reactions.GetCommentReactorsAsync(id, value, page, size, ct);
            return Ok(new { total, items });
        }

        // 3) Beğen / Dislike
        [Authorize]
        [HttpPost("{id:long}/react")]
        public async Task<IActionResult> React(long id, [FromBody] ReactReq req, CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            await _reactions.ReactToCommentAsync(uid, id, req.Value, ct);
            return NoContent();
        }

        // 4) Reaksiyonu kaldır
        [Authorize]
        [HttpDelete("{id:long}/react")]
        public async Task<IActionResult> Unreact(long id, CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            await _reactions.RemoveFromCommentAsync(uid, id, ct);
            return NoContent();
        }
        // GET /api/Comments/{id}
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id, CancellationToken ct)
        {
            var viewer = _current.UserId ?? Guid.Empty;
            var dto = await _comments.GetByIdAsync(id, viewer, ct);
            return dto is null ? NotFound() : Ok(dto);
        }
        // PUT /api/Comments/{id}
        [Authorize]
        [HttpPut("{id:long}")]
        public async Task<IActionResult> Edit(long id, [FromBody] EditReq req, CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            var dto = await _comments.EditAsync(uid, new EditCommentCommand(id, req.Body), ct);
            return Ok(dto);
        }
        // DELETE /api/Comments/{id}
        [Authorize]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            await _comments.DeleteAsync(uid, id, isModerator: false, ct);
            return NoContent();
        }
       
    }
}
