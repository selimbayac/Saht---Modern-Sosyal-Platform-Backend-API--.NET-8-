using Microsoft.EntityFrameworkCore;
using Saht.Application.Abstractions;
using Saht.Domain.Common;
using Saht.Domain.Reactions;
using Saht.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Saht.Infrastructure.Repositories
{
    public sealed class EFReactionRepository : IReactionRepository
    {
        private readonly SahtDbContext _db;
        public EFReactionRepository(SahtDbContext db) => _db = db;

        public Task<Reaction?> GetAsync(ContentType type, long targetId, Guid userId, CancellationToken ct = default) =>
            _db.Reactions.FirstOrDefaultAsync(r =>
                r.TargetType == type && r.TargetId == targetId && r.UserId == userId, ct);

        public async Task UpsertAsync(Reaction reaction, CancellationToken ct = default)
        {
            var tracked = await _db.Reactions.FirstOrDefaultAsync(r =>
            r.TargetType == reaction.TargetType &&
            r.TargetId == reaction.TargetId &&
            r.UserId == reaction.UserId, ct);

            if (tracked is null)
                await _db.Reactions.AddAsync(reaction, ct);
            else
                _db.Entry(tracked).CurrentValues.SetValues(reaction);
        }

        public async Task RemoveAsync(ContentType type, long targetId, Guid userId, CancellationToken ct = default)
        {
            var r = await GetAsync(type, targetId, userId, ct);
            if (r != null) _db.Reactions.Remove(r);
        }

        public async Task<(int likes, int dislikes)> CountAsync(ContentType type, long targetId, CancellationToken ct = default)
        {
            var q = _db.Reactions.Where(r => r.TargetType == type && r.TargetId == targetId);
            var likes = await q.CountAsync(r => r.Value == 1, ct);
            var dislikes = await q.CountAsync(r => r.Value == -1, ct);
            return (likes, dislikes);
        }
        public async Task<(IReadOnlyList<(Guid, string, string, int, DateTime)>, int)> GetReactorsAsync(
               ContentType type, long targetId, int? value, int skip, int take, CancellationToken ct)
        {
            var q = from r in _db.Reactions
                    join u in _db.Users on r.UserId equals u.Id
                    where r.TargetType == type && r.TargetId == targetId
                    select new { u.Id, u.UserName, u.DisplayName, r.Value, r.CreatedAt };

            if (value is 1 or -1) q = q.Where(x => x.Value == value.Value);

            var total = await q.CountAsync(ct);
            var items = await q.OrderByDescending(x => x.CreatedAt)
                               .Skip(skip).Take(take)
                               .Select(x => new ValueTuple<Guid, string, string, int, DateTime>(x.Id, x.UserName, x.DisplayName, x.Value, x.CreatedAt))
                               .ToListAsync(ct);
            return (items, total);
        }
        public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}
