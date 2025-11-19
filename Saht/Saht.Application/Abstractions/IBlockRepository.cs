using Saht.Domain.Social;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Abstractions
{
    public interface IBlockRepository
    {
        Task AddAsync(Block b, CancellationToken ct = default);
        Task RemoveAsync(Guid blockerId, Guid blockedId, CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid blockerId, Guid blockedId, CancellationToken ct = default);
        Task<IReadOnlyList<Guid>> GetBlockedIdsAsync(Guid blockerId, CancellationToken ct = default);
        Task<bool> IsBlockedEitherWayAsync(Guid a, Guid b, CancellationToken ct = default); // (a->b) or (b->a)
        Task<IReadOnlyList<Guid>> GetBlockedEitherWayIdsAsync(Guid userId, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
