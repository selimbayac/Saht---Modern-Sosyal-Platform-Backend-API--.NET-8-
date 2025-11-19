using Saht.Domain.Common;
using Saht.Domain.Reactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Reactions
{
    public interface IReactionService
    {
        Task ReactToPostAsync(Guid userId, ReactCommand cmd, CancellationToken ct = default);
        Task RemoveFromPostAsync(Guid userId, long postId, CancellationToken ct = default);
        Task<ReactionSummaryDto> GetPostSummaryAsync(Guid userIdOrEmpty, long postId, CancellationToken ct = default);
        Task<(IReadOnlyList<ReactionUserDto> Items, int Total)> GetPostReactorsAsync(long postId, int? value, int page, int size, CancellationToken ct = default);

        //comment yani yorum
        Task ReactToCommentAsync(Guid userId, long commentId, int value, CancellationToken ct = default);
        Task RemoveFromCommentAsync(Guid userId, long commentId, CancellationToken ct = default);
        Task<ReactionSummaryDto> GetCommentSummaryAsync(Guid viewerOrEmpty, long commentId, CancellationToken ct = default);
        Task<(IReadOnlyList<ReactionUserDto> Items, int Total)> GetCommentReactorsAsync(long commentId, int? value, int page, int size, CancellationToken ct = default);
        // ==== BLOGPOST (YENİ) ====
        Task ReactToBlogPostAsync(Guid userId, long blogPostId, int value, CancellationToken ct = default);
        Task RemoveFromBlogPostAsync(Guid userId, long blogPostId, CancellationToken ct = default);
        Task<ReactionSummaryDto> GetBlogPostSummaryAsync(Guid viewerOrEmpty, long blogPostId, CancellationToken ct = default);
        Task<(IReadOnlyList<ReactionUserDto> Items, int Total)> GetBlogPostReactorsAsync(long blogPostId, int? value, int page, int size, CancellationToken ct = default);

    }
}
