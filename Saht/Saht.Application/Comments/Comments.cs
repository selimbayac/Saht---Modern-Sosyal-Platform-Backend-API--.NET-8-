using Saht.Application.Reactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Comments
{
    public sealed record CommentDto(
         long Id,
         string Target,            // "Post" | "BlogPost"
         long TargetId,
         int TargetType,           // (int)ContentType
         Guid UserId,
         string UserName,
         string DisplayName,
         string Body,
         long? ParentCommentId,
         DateTime CreatedAt,
         DateTime? EditedAt,
         bool IsDeleted,
         ReactionSummaryDto? Reactions // null olabilir (anonim viewer)
     );

    // Komutlar
    public sealed record CreateOnPostCommand(long PostId, string Body);
    // Blog gelince: public sealed record CreateOnBlogPostCommand(long BlogPostId, string Body);

    public sealed record ReplyCommentCommand(long ParentCommentId, string Body);
    public sealed record EditCommentCommand(long CommentId, string Body);

    // Listeleme için Paged
    public sealed record Paged<T>(int Total, IReadOnlyList<T> Items);
}

