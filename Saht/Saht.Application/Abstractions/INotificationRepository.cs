using Saht.Domain.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Abstractions
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification n, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);

        Task<(IReadOnlyList<Notification> Items, int Total)>
            ListForUserAsync(Guid userId, int skip, int take, CancellationToken ct = default);

        Task<int> CountUnreadAsync(Guid userId, CancellationToken ct = default);
        Task<Notification?> GetByIdAsync(long id, CancellationToken ct = default);
        Task<bool> IsOwnerAsync(long id, Guid userId, CancellationToken ct = default);

        Task MarkReadAsync(long id, CancellationToken ct = default);
        Task MarkAllReadAsync(Guid userId, CancellationToken ct = default);

        // basit dedupe (aynı aktör aynı hedef kısa sürede aynı tür)
        Task<bool> ExistsRecentAsync(Guid userId, NotificationType type, string payloadFingerprint, TimeSpan window, CancellationToken ct = default);
    }
}
