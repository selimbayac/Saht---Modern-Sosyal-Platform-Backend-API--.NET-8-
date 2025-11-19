using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Domain.Notifications
{
    public sealed class Notification : Saht.Domain.Common.AggregateRoot<long>
    {
        private Notification() { }

        public Guid UserId { get; private set; }              // Kime gidecek
        public NotificationType Type { get; private set; }
        public string Payload { get; private set; } = null!;  // JSON: { postId:123, fromUser:"selim" } gibi
        public bool IsRead { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; private set; }

        public static Notification Create(Guid userId, NotificationType type, string payload)
        {
            if (userId == Guid.Empty) throw new ArgumentException(nameof(userId));
            if (string.IsNullOrWhiteSpace(payload)) throw new ArgumentException(nameof(payload));

            return new Notification
            {
                Id = 0,
                UserId = userId,
                Type = type,
                Payload = payload
            };
        }

        public void MarkRead()
        {
            if (IsRead) return;
            IsRead = true;
            ReadAt = DateTime.UtcNow;
        }
    }
}
