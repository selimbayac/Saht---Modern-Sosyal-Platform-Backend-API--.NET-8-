using Saht.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Domain.Reactions
{
    public sealed class Reaction
    {
        private Reaction() { }

        public ContentType TargetType { get; private set; }
        public long TargetId { get; private set; }
        public Guid UserId { get; private set; }
        public int Value { get; private set; } // +1 / -1
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }

        public static Reaction Give(Guid userId, ContentType type, long targetId, int value)
        {
            Ensure(value, targetId);
            return new Reaction { UserId = userId, TargetType = type, TargetId = targetId, Value = value };
        }

        public void Change(int newValue)
        {
            Ensure(newValue, TargetId);
            if (Value == newValue) return;
            Value = newValue;
            UpdatedAt = DateTime.UtcNow;
        }

        private static void Ensure(int v, long targetId)
        {
            if (targetId <= 0) throw new ArgumentOutOfRangeException(nameof(targetId));
            if (v != 1 && v != -1) throw new ArgumentOutOfRangeException(nameof(v), "Değer +1/-1 olmalı");
        }
    }
}
