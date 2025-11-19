using Saht.Application.Reactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Posts
{
    public sealed record PostDto(
     long Id, string Type, string? Body,
     long? ParentPostId,
     Guid AuthorId, string AuthorUserName, string AuthorDisplayName,
     DateTime CreatedAt, DateTime? EditedAt,
     ReactionSummaryDto Reactions);
}
