using Saht.Domain.Posts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Saht.Application.Posts.PostCommands;

namespace Saht.Application.Posts
{
    public interface IPostService
    {
        Task<PostDto> CreateAsync(Guid authorId, CreatePostCommand cmd, CancellationToken ct = default);
        Task<PostDto> ReplyAsync(Guid authorId, ReplyCommand cmd, CancellationToken ct = default);
        Task<PostDto> QuoteAsync(Guid authorId, QuoteCommand cmd, CancellationToken ct = default);
        Task<PostDto> RepostAsync(Guid authorId, RepostCommand cmd, CancellationToken ct = default);

        Task<PostDto?> GetByIdAsync(long id, Guid viewerId, CancellationToken ct = default);
        //Task<IReadOnlyList<PostDto>> GetFeedAsync(Guid viewerId, int page, int size, CancellationToken ct = default);
        //Task<IReadOnlyList<PostDto>> GetUserTimelineAsync(Guid targetUserId, Guid viewerId, int page, int size, CancellationToken ct = default);

        Task<PostDto> EditAsync(Guid authorId, EditPostCommand cmd, CancellationToken ct = default);
        Task DeleteAsync(Guid authorId, long postId, bool isModerator = false, CancellationToken ct = default);
        Task<PagedList<PostDto>> GetFeedAsync(Guid viewerId, int page, int size, CancellationToken ct);
        Task<PagedList<PostDto>> GetUserTimelineAsync(Guid targetUserId, Guid viewerId, int page, int size, CancellationToken ct);
        Task<PagedList<PostDto>> GetPublicFeedAsync(Guid viewerId, int page, int size, CancellationToken ct);
    }

}

