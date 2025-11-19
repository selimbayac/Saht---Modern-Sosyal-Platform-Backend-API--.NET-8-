using Saht.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Domain.Comments
{
        public sealed class Comment : AggregateRoot<long>
        {
            private Comment() { }

            public ContentType TargetType { get; private set; } // Post | BlogPost
            public long TargetId { get; private set; }          // Hedefin Id’si (Post.Id veya BlogPost.Id)

            public Guid UserId { get; private set; }            // Kim yazdı
            public string Body { get; private set; } = null!;

            public long? ParentCommentId { get; private set; }  // Threading
            public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
            public DateTime? EditedAt { get; private set; }
            public bool IsDeleted { get; private set; }

            public static Comment CreateRoot(ContentType targetType, long targetId, Guid userId, string body)
            {
                Ensure(body, targetId);
                return new Comment
                {
                    Id = 0,
                    TargetType = targetType,
                    TargetId = targetId,
                    UserId = userId,
                    Body = body.Trim()
                };
            }

            public static Comment CreateReply(ContentType targetType, long targetId, Guid userId, long parentId, string body)
            {
                Ensure(body, targetId);
                if (parentId <= 0) throw new ArgumentOutOfRangeException(nameof(parentId));
                return new Comment
                {
                    Id = 0,
                    TargetType = targetType,
                    TargetId = targetId,
                    UserId = userId,
                    Body = body.Trim(),
                    ParentCommentId = parentId
                };
            }

            public void Edit(string newBody)
            {
                if (IsDeleted) throw new InvalidOperationException("Silinmiş yorum düzenlenemez.");
                if (string.IsNullOrWhiteSpace(newBody)) throw new ArgumentException(nameof(newBody));
                Body = newBody.Trim();
                EditedAt = DateTime.UtcNow;
            }

            public void SoftDelete()
            {
                IsDeleted = true;
                Body = "[deleted]";
            }

            private static void Ensure(string body, long targetId)
            {
                if (string.IsNullOrWhiteSpace(body)) throw new ArgumentException(nameof(body));
                if (targetId <= 0) throw new ArgumentOutOfRangeException(nameof(targetId));
            }
        }
}
