using Saht.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Domain.Posts
{
    public sealed class Post : AggregateRoot<long>
    {
        private Post() { }
        public Guid AuthorId { get; private set; }
        public PostType Type { get; private set; } = PostType.Normal;
        public string? Body { get; private set; }
        public long? ParentPostId { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? EditedAt { get; private set; }
        public bool IsDeleted { get; private set; }

        public static Post CreateNormal(Guid authorId, string body) =>
            new() { AuthorId = authorId, Body = body.Trim(), Type = PostType.Normal };

        public static Post CreateRepost(Guid authorId, long parentPostId) =>
            new() { AuthorId = authorId, ParentPostId = parentPostId, Type = PostType.Repost };

        public static Post CreateQuote(Guid authorId, long parentPostId, string body) =>
            new() { AuthorId = authorId, ParentPostId = parentPostId, Body = body.Trim(), Type = PostType.Quote };

        public void Edit(string newBody) { if (Type == PostType.Repost) throw new InvalidOperationException(); Body = newBody.Trim(); EditedAt = DateTime.UtcNow; }
        public void SoftDelete() { IsDeleted = true; Body = null; }
        public static Post CreateReply(Guid authorId, long parentPostId, string body)  => new() { AuthorId = authorId, ParentPostId = parentPostId, Body = body.Trim(),Type = PostType.Reply

    };

    }
}
