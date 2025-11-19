using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saht.Api.Requests;
using Saht.Application.Abstractions;
using Saht.Application.Blogs;
using Saht.Application.Reactions;

namespace Saht.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public sealed class BlogPostsController : ControllerBase
    {
        private readonly IBlogPostService _blogs;
        private readonly ICurrentUser _current;
        private readonly IReactionService _reactions;
        public BlogPostsController(IBlogPostService blogs, ICurrentUser current, IReactionService reactionService)
        { _blogs = blogs; _current = current; _reactions = reactionService; }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBlog req, CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            var dto = await _blogs.CreateAsync(uid, new CreateBlogPostCommand(req.Title, req.Body), ct);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id, CancellationToken ct)
        {
            var dto = await _blogs.GetByIdAsync(id, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> ListByUser(Guid userId, [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
            => Ok(await _blogs.ListByUserAsync(userId, page, size, ct));

        [HttpGet("recent")]
        public async Task<IActionResult> Recent([FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
            => Ok(await _blogs.ListRecentAsync(page, size, ct));

        [Authorize]
        [HttpPut("{id:long}")]
        public async Task<IActionResult> Edit(long id, [FromBody] CreateBlog req, CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            var dto = await _blogs.EditAsync(uid, new EditBlogPostCommand(id, req.Title, req.Body), ct);
            return Ok(dto);
        }

        [Authorize]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            await _blogs.DeleteAsync(uid, id, isModerator: false, ct);
            return NoContent();
        }

        // --- Comments on BlogPost ---
        [Authorize]
        [HttpPost("{blogId:long}/comments")]
        public async Task<IActionResult> CreateComment(long blogId, [FromBody] BodyReq req, CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            var dto = await _blogs.CreateCommentAsync(uid, blogId, req.Body, ct);
            return Ok(dto);
        }

        [HttpGet("{blogId:long}/comments")]
        public async Task<IActionResult> ListComments(long blogId, [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
        {
            var viewer = _current.UserId ?? Guid.Empty;
            var paged = await _blogs.ListCommentsAsync(blogId, page, size, viewer, ct);
            return Ok(paged);
        }
        [Authorize]
        [HttpPost("{id:long}/react")]
        public async Task<IActionResult> React(long id, [FromBody] ReactReq req, CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            await _reactions.ReactToBlogPostAsync(uid, id, req.Value, ct);
            return NoContent();
        }

        // DELETE /api/BlogPosts/{id}/react
        [Authorize]
        [HttpDelete("{id:long}/react")]
        public async Task<IActionResult> Unreact(long id, CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            await _reactions.RemoveFromBlogPostAsync(uid, id, ct);
            return NoContent();
        }

        // GET /api/BlogPosts/{id}/reactions/summary
        [HttpGet("{id:long}/reactions/summary")]
        public async Task<IActionResult> Summary(long id, CancellationToken ct)
        {
            var viewer = _current.UserId ?? Guid.Empty;
            var dto = await _reactions.GetBlogPostSummaryAsync(viewer, id, ct);
            return Ok(dto);
        }
        [HttpGet("{id:long}/reactions")]
        public async Task<IActionResult> Reactors(long id, [FromQuery] int? value, [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
        {
            var (items, total) = await _reactions.GetBlogPostReactorsAsync(id, value, page, size, ct);
            return Ok(new { total, items });
        }
    }
}

