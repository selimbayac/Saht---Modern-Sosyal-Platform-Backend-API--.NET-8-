using Saht.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Domain.Blogs
{
    public sealed class BlogPostVote : Entity<(Guid BlogPostId, Guid UserId)>
    {
        private BlogPostVote() { }

        public Guid BlogPostId { get; private set; }
        public Guid UserId { get; private set; }
        public int Value { get; private set; } // +1 / -1
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }

        public static BlogPostVote Create(Guid blogPostId, Guid userId, int value)
        {
            Ensure(value);
            return new BlogPostVote
            {
                Id = (blogPostId, userId),
                BlogPostId = blogPostId,
                UserId = userId,
                Value = value
            };
        }

        public void Change(int newValue)
        {
            Ensure(newValue);
            if (Value == newValue) return;
            Value = newValue;
            UpdatedAt = DateTime.UtcNow;
        }

        private static void Ensure(int v)
        {
            if (v != 1 && v != -1)
                throw new ArgumentOutOfRangeException(nameof(v), "Oy +1 ya da -1 olmalı.");
        }
    }
}
