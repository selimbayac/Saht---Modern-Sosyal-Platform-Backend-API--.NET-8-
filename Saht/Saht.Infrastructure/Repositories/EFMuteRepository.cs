using Microsoft.EntityFrameworkCore;
using Saht.Application.Abstractions;
using Saht.Domain.Social;
using Saht.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Infrastructure.Repositories
{
    public sealed class EFMuteRepository : IMuteRepository
    {
        private readonly SahtDbContext _db;
        public EFMuteRepository(SahtDbContext db) => _db = db;

        public Task AddAsync(Mute m, CancellationToken ct = default)
            => _db.Mutes.AddAsync(m, ct).AsTask();

        public async Task RemoveAsync(Guid muterId, Guid mutedId, CancellationToken ct = default)
        {
            var entity = await _db.Mutes.FindAsync(new object[] { muterId, mutedId }, ct);
            if (entity != null) _db.Mutes.Remove(entity);
        }

        public Task<bool> ExistsAsync(Guid muterId, Guid mutedId, CancellationToken ct = default)
            => _db.Mutes.AnyAsync(x => x.MuterId == muterId && x.MutedId == mutedId, ct);

        public async Task<IReadOnlyList<Guid>> GetMutedIdsAsync(Guid muterId, CancellationToken ct = default)
            => await _db.Mutes.Where(x => x.MuterId == muterId).Select(x => x.MutedId).ToListAsync(ct);

        public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}
