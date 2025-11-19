using Microsoft.EntityFrameworkCore;
using Saht.Application.Abstractions;
using Saht.Domain.Follows;
using Saht.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Infrastructure.Repositories
{
    public sealed class EFFollowRepository : IFollowRepository
    {
        private readonly SahtDbContext _db;
        public EFFollowRepository(SahtDbContext db) => _db = db;

        public Task<bool> ExistsAsync(Guid followerId, Guid followeeId, CancellationToken ct = default)
            => _db.Follows.AnyAsync(x => x.FollowerId == followerId && x.FolloweeId == followeeId, ct);

        public async Task AddAsync(Follow f, CancellationToken ct = default)
            => await _db.Follows.AddAsync(f, ct);

        public async Task RemoveAsync(Guid followerId, Guid followeeId, CancellationToken ct = default)
        {
            var ent = await _db.Follows.FirstOrDefaultAsync(x => x.FollowerId == followerId && x.FolloweeId == followeeId, ct);
            if (ent != null) _db.Follows.Remove(ent);
        }

        public Task<int> CountFollowersAsync(Guid userId, CancellationToken ct = default)
            => _db.Follows.CountAsync(x => x.FolloweeId == userId, ct);

        public Task<int> CountFollowingAsync(Guid userId, CancellationToken ct = default)
            => _db.Follows.CountAsync(x => x.FollowerId == userId, ct);

        public async Task<IReadOnlyList<Guid>> GetFollowingIdsAsync(Guid userId, int skip, int take, CancellationToken ct = default)
            => await _db.Follows.Where(x => x.FollowerId == userId)
                                .OrderByDescending(x => x.CreatedAt)
                                .Skip(skip).Take(take)
                                .Select(x => x.FolloweeId)
                                .ToListAsync(ct);

        public async Task<IReadOnlyList<Guid>> GetFollowerIdsAsync(Guid userId, int skip, int take, CancellationToken ct = default)
            => await _db.Follows.Where(x => x.FolloweeId == userId)
                                .OrderByDescending(x => x.CreatedAt)
                                .Skip(skip).Take(take)
                                .Select(x => x.FollowerId)
                                .ToListAsync(ct);
        public async Task<IReadOnlyList<Guid>> GetFollowerIdsOfAsync(Guid userId, CancellationToken ct = default)
        {
            // FolloweeId = userId olan kayıtların FollowerId’lerini getir
            return await _db.Follows
                .Where(f => f.FolloweeId == userId)
                .Select(f => f.FollowerId)
                .ToListAsync(ct);
        }
        public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    }
}
