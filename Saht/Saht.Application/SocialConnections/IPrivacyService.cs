using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.SocialConnections
{
    public interface IPrivacyService
    {
        Task BlockAsync(Guid me, Guid target, CancellationToken ct = default);
        Task UnblockAsync(Guid me, Guid target, CancellationToken ct = default);
        Task<bool> IsBlockedEitherWayAsync(Guid a, Guid b, CancellationToken ct = default);

        Task MuteAsync(Guid me, Guid target, CancellationToken ct = default);
        Task UnmuteAsync(Guid me, Guid target, CancellationToken ct = default);
        Task<IReadOnlyList<Guid>> GetMutedAsync(Guid me, CancellationToken ct = default);
        Task<IReadOnlyList<Guid>> GetBlockedAsync(Guid me, CancellationToken ct = default);
        Task<IReadOnlyList<Guid>> GetBlockedIdsAsync(Guid viewerId, CancellationToken ct = default);
    }
}
