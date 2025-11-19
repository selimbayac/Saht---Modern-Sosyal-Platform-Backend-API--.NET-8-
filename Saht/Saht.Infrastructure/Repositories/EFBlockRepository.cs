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
    public sealed class EFBlockRepository : IBlockRepository
    {
        private readonly SahtDbContext _db;
        public EFBlockRepository(SahtDbContext db) => _db = db;

        public Task AddAsync(Block b, CancellationToken ct = default)
            => _db.Blocks.AddAsync(b, ct).AsTask();

        public async Task RemoveAsync(Guid blockerId, Guid blockedId, CancellationToken ct = default)
        {
            var entity = await _db.Blocks.FindAsync(new object[] { blockerId, blockedId }, ct);
            if (entity != null) _db.Blocks.Remove(entity);
        }

        public Task<bool> ExistsAsync(Guid blockerId, Guid blockedId, CancellationToken ct = default)
            => _db.Blocks.AnyAsync(x => x.BlockerId == blockerId && x.BlockedId == blockedId, ct);

        public async Task<IReadOnlyList<Guid>> GetBlockedIdsAsync(Guid blockerId, CancellationToken ct = default)
            => await _db.Blocks.Where(x => x.BlockerId == blockerId).Select(x => x.BlockedId).ToListAsync(ct);

        public Task<bool> IsBlockedEitherWayAsync(Guid a, Guid b, CancellationToken ct = default)
            => _db.Blocks.AnyAsync(x =>
                  (x.BlockerId == a && x.BlockedId == b) ||
                  (x.BlockerId == b && x.BlockedId == a), ct);

        public async Task<IReadOnlyList<Guid>> GetBlockedEitherWayIdsAsync(Guid userId, CancellationToken ct = default)
        {
            // 1. userId'nin engellediği kişiler (BlockerId = userId)
            var blockedByMe = _db.Blocks
                .Where(b => b.BlockerId == userId)
                .Select(b => b.BlockedId);

            // 2. userId'yi engelleyen kişiler (BlockedId = userId)
            var blockingMe = _db.Blocks
                .Where(b => b.BlockedId == userId)
                .Select(b => b.BlockerId);

            // 3. İki listeyi birleştir ve tekrarları kaldır (UNION)
            return await blockedByMe
                .Union(blockingMe)              
                .ToListAsync(ct);
        }
    
        public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}
