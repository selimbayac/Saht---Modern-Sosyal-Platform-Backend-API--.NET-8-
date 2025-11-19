using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Domain.Social
{
    public sealed class Mute
    {
        private Mute() { }
        public Guid MuterId { get; private set; }
        public Guid MutedId { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public static Mute Create(Guid muter, Guid muted)
        {
            if (muter == Guid.Empty || muted == Guid.Empty) throw new ArgumentException();
            if (muter == muted) throw new InvalidOperationException("Kendini susturamazsın");
            return new Mute { MuterId = muter, MutedId = muted };
        }
    }
}
