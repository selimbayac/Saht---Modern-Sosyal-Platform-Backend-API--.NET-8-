using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saht.Api.Requests;
using Saht.Application.Abstractions;
using Saht.Application.Comments;
using Saht.Application.Posts;
using Saht.Application.Reactions;
using Saht.Domain.Reactions;
using static Saht.Application.Posts.PostCommands;

namespace Saht.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class PostsController : ControllerBase
    {
        private readonly IPostService _posts;
        private readonly ICurrentUser _current;
        private readonly IReactionService _reactions;
        private readonly ICommentService _commentService;
        public PostsController(IPostService posts, ICurrentUser current, IReactionService reactions, ICommentService commentService)
        {
            _posts = posts; _current = current;
            _reactions = reactions;
            _commentService = commentService;
        }

        // CREATE
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReq req, CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            var dto = await _posts.CreateAsync(uid, new CreatePostCommand(req.Body), ct);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }
        // HERKESE AÇIK GENEL AKIŞ
        [HttpGet("public")]
        public async Task<IActionResult> PublicFeed([FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
        {
            var viewer = _current.UserId ?? Guid.Empty; // login ise id'yi geçersin
            var list = await _posts.GetPublicFeedAsync(viewer, page, size, ct);
            return Ok(list);
        }

        // REPLY
        [Authorize]
        [HttpPost("{id:long}/reply")]
        public async Task<IActionResult> Reply(long id, [FromBody] BodyReq req, CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            var dto = await _posts.ReplyAsync(uid, new ReplyCommand(id, req.Body), ct);
            return Ok(dto);
        }

        // QUOTE
        [Authorize]
        [HttpPost("{id:long}/quote")]
        public async Task<IActionResult> Quote(long id, [FromBody] BodyReq req, CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            var dto = await _posts.QuoteAsync(uid, new QuoteCommand(id, req.Body), ct);
            return Ok(dto);
        }

        // REPOST
        [Authorize]
        [HttpPost("{id:long}/repost")]
        public async Task<IActionResult> Repost(long id, CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            var dto = await _posts.RepostAsync(uid, new RepostCommand(id), ct);
            return Ok(dto);
        }

        // GET BY ID
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id, CancellationToken ct)
        {
            var viewer = _current.UserId ?? Guid.Empty;
            var dto = await _posts.GetByIdAsync(id, viewer, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        // FEED
        [Authorize]
        [HttpGet("feed")]
        public async Task<IActionResult> Feed([FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            var list = await _posts.GetFeedAsync(uid, page, size, ct);
            return Ok(list);
        }

        // USER TIMELINE (username --> userId’e çevirip çağırırsın; şimdilik id kullanalım)
        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> UserTimeline(Guid userId, [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
        {
            var viewer = _current.UserId ?? Guid.Empty; // anonim olabilir
            var list = await _posts.GetUserTimelineAsync(userId, viewer, page, size, ct);
            return Ok(list);
        }

        // EDIT
        [Authorize]
        [HttpPut("{id:long}")]
        public async Task<IActionResult> Edit(long id, [FromBody] BodyReq req, CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            var dto = await _posts.EditAsync(uid, new EditPostCommand(id, req.Body), ct);
            return Ok(dto);
        }

        // DELETE (soft)
        [Authorize]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            await _posts.DeleteAsync(uid, id, isModerator: false, ct);
            return NoContent();
        }
        [Authorize]
        [HttpPost("{id:long}/react")]
        public async Task<IActionResult> React(long id, [FromBody] ReactReq req, CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            await _reactions.ReactToPostAsync(uid, new ReactCommand(id, req.Value), ct);
            return NoContent();
        }
        [Authorize]
        [HttpDelete("{id:long}/react")]
        public async Task<IActionResult> Unreact(long id, CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();
            await _reactions.RemoveFromPostAsync(uid, id, ct);
            return NoContent();
        }
        [HttpGet("{id:long}/reactions/summary")]
        public async Task<IActionResult> Summary(long id, CancellationToken ct)
        {
            var uid = _current.UserId ?? Guid.Empty; // anonimse boş
            var dto = await _reactions.GetPostSummaryAsync(uid, id, ct);
            return Ok(dto);
        }
        // GET /api/Posts/{id}/reactions?value=1|-1&page=1&size=20
        [HttpGet("{id:long}/reactions")]
        public async Task<IActionResult> Reactors(long id, [FromQuery] int? value, [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
        {
            var (items, total) = await _reactions.GetPostReactorsAsync(id, value, page, size, ct);
            return Ok(new { total, items });
        }

        // POST /api/Posts/{postId}/comments (yorum oluştur)
        [Authorize]
        [HttpPost("{postId:long}/comments")]
        public async Task<IActionResult> CreateComment(
             long postId,
             [FromBody] BodyReq req,
             CancellationToken ct)
        {
            var uid = _current.UserId ?? throw new UnauthorizedAccessException();

            // ÖNEMLİ DÜZELTME: Artık merkezi metot CreateOnTargetAsync kullanılıyor.
            var dto = await _commentService.CreateOnTargetAsync(
                uid,
                Saht.Domain.Common.ContentType.Post, // Hedefin Post olduğunu belirtiyoruz
                postId,
                req.Body,
                ct);

            // Başarılı yanıt: Yeni yorumun Comment ID'si ile CommentsController'daki GetById'a yönlendir.
            return CreatedAtAction(
                nameof(CommentsController.GetById),
                "Comments", // Yönlendirilecek Controller Adı
                new { id = dto.Id },
                dto);
        }
        // GET /api/Posts/{postId}/comments?page=&size= (yorumları listele)
        [HttpGet("{postId:long}/comments")]
        public async Task<IActionResult> ListComments(long postId, [FromQuery] int page = 1, [FromQuery] int size = 20, [FromServices] ICommentService comments = null!, CancellationToken ct = default)
        {
            var viewer = _current.UserId ?? Guid.Empty; // anonim olabilir
            var paged = await comments.ListForTargetAsync(Saht.Domain.Common.ContentType.Post, postId, page, size, viewer, ct);
            return Ok(paged);
        }  
    }
}
