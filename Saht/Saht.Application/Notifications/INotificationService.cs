using Saht.Application.Comments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Notifications
{
    public interface INotificationService
    {
        Task<Paged<NotificationDto>> ListAsync(Guid userId, int page, int size, CancellationToken ct = default);
        Task<int> UnreadCountAsync(Guid userId, CancellationToken ct = default);
        Task MarkReadAsync(Guid userId, long id, CancellationToken ct = default);
        Task MarkAllReadAsync(Guid userId, CancellationToken ct = default);

        Task NewFollowerAsync(Guid toUserId, Guid actorId, CancellationToken ct = default);
        Task PostLikedAsync(Guid toUserId, Guid actorId, long postId, CancellationToken ct = default);
        Task PostCommentedAsync(Guid toUserId, Guid actorId, long postId, CancellationToken ct = default);
        Task ReportDecisionAsync(Guid toUserId, string metaJson, CancellationToken ct = default);
    }
}
