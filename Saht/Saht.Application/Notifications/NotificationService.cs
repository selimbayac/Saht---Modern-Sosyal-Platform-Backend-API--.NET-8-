using Saht.Application.Abstractions;
using Saht.Application.Comments;
using Saht.Domain.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Saht.Application.Notifications
{
    public sealed class NotificationService : INotificationService
    {
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly INotificationRepository _repo;

        public NotificationService(INotificationRepository repo)
        {
            _repo = repo;
        }

        public async Task<Paged<NotificationDto>> ListAsync(Guid userId, int page, int size, CancellationToken ct = default)
        {
            var (skip, take) = Paginate(page, size);
            var (items, total) = await _repo.ListForUserAsync(userId, skip, take, ct);
            var list = items.Select(n => new NotificationDto(n.Id, n.Type.ToString(), n.Payload, n.IsRead, n.CreatedAt, n.ReadAt)).ToList();
            return new Paged<NotificationDto>(total, list);
        }

        public Task<int> UnreadCountAsync(Guid userId, CancellationToken ct = default)
            => _repo.CountUnreadAsync(userId, ct);

        public async Task MarkReadAsync(Guid userId, long id, CancellationToken ct = default)
        {
            var owner = await _repo.IsOwnerAsync(id, userId, ct);
            if (!owner) throw new UnauthorizedAccessException();
            await _repo.MarkReadAsync(id, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task MarkAllReadAsync(Guid userId, CancellationToken ct = default)
        {
            await _repo.MarkAllReadAsync(userId, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task NewFollowerAsync(Guid toUserId, Guid actorId, CancellationToken ct = default)
        {
            if (toUserId == actorId) return;
            var payload = JsonSerializer.Serialize(new { actorId }, JsonOpts);
            var fp = Fingerprint(NotificationType.NewFollower, payload);

            if (await _repo.ExistsRecentAsync(toUserId, NotificationType.NewFollower, fp, TimeSpan.FromHours(24), ct))
                return;

            await _repo.AddAsync(Notification.Create(toUserId, NotificationType.NewFollower, fp), ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task PostLikedAsync(Guid toUserId, Guid actorId, long postId, CancellationToken ct = default)
        {
            if (toUserId == actorId) return;
            var payload = JsonSerializer.Serialize(new { actorId, postId }, JsonOpts);
            var fp = Fingerprint(NotificationType.PostLiked, payload);

            if (await _repo.ExistsRecentAsync(toUserId, NotificationType.PostLiked, fp, TimeSpan.FromHours(6), ct))
                return;

            await _repo.AddAsync(Notification.Create(toUserId, NotificationType.PostLiked, fp), ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task PostCommentedAsync(Guid toUserId, Guid actorId, long postId, CancellationToken ct = default)
        {
            if (toUserId == actorId) return;
            var payload = JsonSerializer.Serialize(new { actorId, postId }, JsonOpts);
            var fp = Fingerprint(NotificationType.PostCommented, payload);

            if (await _repo.ExistsRecentAsync(toUserId, NotificationType.PostCommented, fp, TimeSpan.FromHours(2), ct))
                return;

            await _repo.AddAsync(Notification.Create(toUserId, NotificationType.PostCommented, fp), ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ReportDecisionAsync(Guid toUserId, string metaJson, CancellationToken ct = default)
        {
            var fp = Fingerprint(NotificationType.ReportDecision, metaJson);
            await _repo.AddAsync(Notification.Create(toUserId, NotificationType.ReportDecision, fp), ct);
            await _repo.SaveChangesAsync(ct);
        }

        private static string Fingerprint(NotificationType type, string payloadJson)
        {
            // deterministik: tür adı + canonical json
            // (payload’ı zaten camelCase ve stable serializer ile ürettik)
            return $"{type}:{payloadJson}";
        }

        private static (int skip, int take) Paginate(int page, int size)
        {
            page = page <= 0 ? 1 : page;
            size = size is <= 0 or > 50 ? 20 : size;
            return ((page - 1) * size, size);
        }
    }
}