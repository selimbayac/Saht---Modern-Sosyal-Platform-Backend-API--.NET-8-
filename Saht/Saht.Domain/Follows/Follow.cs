using Saht.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Domain.Follows
{
    public sealed class Follow
    {
        private Follow() { }
        public Guid FollowerId { get; private set; }
        public Guid FolloweeId { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public static Follow Create(Guid followerId, Guid followeeId)
        {
            if (followerId == Guid.Empty || followeeId == Guid.Empty) throw new ArgumentException("Boş id");
            if (followerId == followeeId) throw new InvalidOperationException("Kendini takip edemezsin");
            return new Follow { FollowerId = followerId, FolloweeId = followeeId };
        }
    }
}