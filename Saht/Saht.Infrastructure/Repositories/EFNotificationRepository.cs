using Microsoft.EntityFrameworkCore;
using Saht.Application.Abstractions;
using Saht.Domain.Notifications;
using Saht.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Infrastructure.Repositories
{
    public sealed class EFNotificationRepository : INotificationRepository
    {
        private readonly SahtDbContext _db;
        public EFNotificationRepository(SahtDbContext db) => _db = db;

        public Task AddAsync(Notification n, CancellationToken ct = default)
            => _db.Set<Notification>().AddAsync(n, ct).AsTask();

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);

        public async Task<(IReadOnlyList<Notification> Items, int Total)> ListForUserAsync(Guid userId, int skip, int take, CancellationToken ct = default)
        {
            var q = _db.Set<Notification>().Where(x => x.UserId == userId)
                                           .OrderByDescending(x => x.Id);
            var total = await q.CountAsync(ct);
            var items = await q.Skip(skip).Take(take).ToListAsync(ct);
            return (items, total);
        }

        public Task<int> CountUnreadAsync(Guid userId, CancellationToken ct = default)
            => _db.Set<Notification>().CountAsync(x => x.UserId == userId && !x.IsRead, ct);

        public Task<Notification?> GetByIdAsync(long id, CancellationToken ct = default)
            => _db.Set<Notification>().FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<bool> IsOwnerAsync(long id, Guid userId, CancellationToken ct = default)
            => _db.Set<Notification>().AnyAsync(x => x.Id == id && x.UserId == userId, ct);

        public async Task MarkReadAsync(long id, CancellationToken ct = default)
        {
            var n = await GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Bildirim yok");
            n.MarkRead();
        }

        public Task MarkAllReadAsync(Guid userId, CancellationToken ct = default)
            => _db.Set<Notification>()
                  .Where(x => x.UserId == userId && !x.IsRead)
                  .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsRead, true)
                                            .SetProperty(x => x.ReadAt, DateTime.UtcNow), ct);

        public Task<bool> ExistsRecentAsync(Guid userId, NotificationType type, string payloadFingerprint, TimeSpan window, CancellationToken ct = default)
        {
            var since = DateTime.UtcNow - window;
            // fingerprint’i Payload LIKE ile arayabilir ya da tamamen eşitlik olsun diye payload’ı deterministik üretiriz.
            return _db.Set<Notification>().AnyAsync(x =>
                x.UserId == userId &&
                x.Type == type &&
                x.Payload == payloadFingerprint && // deterministik üretirsek birebir eşitlik kullanırız
                x.CreatedAt >= since, ct);
        }
    }
}
