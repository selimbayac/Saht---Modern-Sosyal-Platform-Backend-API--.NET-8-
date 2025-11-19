using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Domain.Social
{
    public sealed class Block
    {
        private Block() { }
        public Guid BlockerId { get; private set; }
        public Guid BlockedId { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public static Block Create(Guid blocker, Guid blocked)
        {
            if (blocker == Guid.Empty || blocked == Guid.Empty) throw new ArgumentException();
            if (blocker == blocked) throw new InvalidOperationException("Kendini engelleyemezsin");
            return new Block { BlockerId = blocker, BlockedId = blocked };
        }
    }
}
