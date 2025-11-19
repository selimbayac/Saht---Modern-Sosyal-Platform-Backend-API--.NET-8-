using Saht.Domain.Social;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Abstractions
{
    public interface IMuteRepository
    {
        Task AddAsync(Mute m, CancellationToken ct = default);
        Task RemoveAsync(Guid muterId, Guid mutedId, CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid muterId, Guid mutedId, CancellationToken ct = default);
        Task<IReadOnlyList<Guid>> GetMutedIdsAsync(Guid muterId, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
