using Saht.Application.Abstractions;
using Saht.Domain.Follows;
using Saht.Domain.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Saht.Application.SocialConnections.IFollowService;

namespace Saht.Application.SocialConnections
{
    public sealed class FollowService : IFollowService
    {
        private readonly IFollowRepository _follows;
        private readonly IUserRepository _users;
        private readonly INotificationRepository _notifs;
        public FollowService(IFollowRepository repo, IUserRepository users, INotificationRepository notifs)
        {
            _follows = repo; _users = users;
            _notifs = notifs;
        }

        public async Task FollowAsync(Guid me, Guid target, CancellationToken ct = default)
        {
            // 1. Giriş Kontrolleri (Hata Kontrolü ve Güvenlik)
            if (me == Guid.Empty || target == Guid.Empty)
                throw new ArgumentException("Takip eden veya edilen kullanıcı kimliği geçersiz.");

            if (me == target)
                throw new InvalidOperationException("Kendinizi takip edemezsiniz.");

            // İdempotent Kontrolü: Zaten takip ediyorsa işlemi sonlandır.
            if (await _follows.ExistsAsync(me, target, ct))
                return;

            // 2. Takip Kaydını Oluşturma
            // Bu satır, Domain katmanınızdaki Follow entity'sinin constructor'ını çağırır.
            var followEntity = Follow.Create(me, target);
            await _follows.AddAsync(followEntity, ct);

            // 3. Yeni Takipçi Bildirimini Oluşturma (Takip edilen kişiye gönderilecek)

            // Takipçinin (me) bilgilerini al
            var follower = await _users.GetByIdAsync(me, ct)
                ?? throw new InvalidOperationException("Takip eden kullanıcı (Follower) bulunamadı.");

            // Payload (Bildirim içeriği) oluştur
            var payload = JsonSerializer.Serialize(new
            {
                followerId = follower.Id,
                followerUserName = follower.UserName,
                followerDisplayName = follower.DisplayName
            });

            // Bildirim entity'sini oluştur
            var notificationEntity = Notification.Create(
                userId: target, // Bildirim alıcısı: Takip edilen (target)
                type: NotificationType.NewFollower,
                payload: payload
            );
            await _notifs.AddAsync(notificationEntity, ct);

            // 4. Veritabanına Kalıcı Kayıt (Persist)
            // Eğer tüm repository'ler (follows ve notifs) aynı DbContext'i paylaşıyorsa,
            // iki işlemi de aynı anda kaydetmek en güvenli yoldur.
            await _follows.SaveChangesAsync(ct);
            await _notifs.SaveChangesAsync(ct);
        }

        public async Task UnfollowAsync(Guid me, Guid target, CancellationToken ct = default)
        {
            if (me == Guid.Empty || target == Guid.Empty) throw new ArgumentException("Geçersiz kullanıcı.");
            if (!await _follows.ExistsAsync(me, target, ct)) return;

            await _follows.RemoveAsync(me, target, ct);
            await _follows.SaveChangesAsync(ct);
        }

        public Task<bool> IsFollowingAsync(Guid me, Guid target, CancellationToken ct = default)
            => _follows.ExistsAsync(me, target, ct);

        public async Task<(int followers, int following)> CountsAsync(Guid userId, CancellationToken ct = default)
        {
            var followers = await _follows.CountFollowersAsync(userId, ct);
            var following = await _follows.CountFollowingAsync(userId, ct);
            return (followers, following);
        }

        public async Task<(IReadOnlyList<UserBriefDto> Items, int Total)> GetFollowersAsync(Guid userId, int page, int size, CancellationToken ct = default)
        {
            var (skip, take) = Paginate(page, size);

            var ids = await _follows.GetFollowerIdsAsync(userId, skip, take, ct);
            var total = await _follows.CountFollowersAsync(userId, ct);

            var items = new List<UserBriefDto>(ids.Count);
            foreach (var id in ids)
            {
                var u = await _users.GetByIdAsync(id, ct);
                if (u != null) items.Add(new UserBriefDto(u.Id, u.UserName, u.DisplayName));
            }
            return (items, total);
        }
        public async Task<(IReadOnlyList<UserBriefDto> Items, int Total)> GetFollowingAsync(Guid userId, int page, int size, CancellationToken ct = default)
        {
            var (skip, take) = Paginate(page, size);

            var ids = await _follows.GetFollowingIdsAsync(userId, skip, take, ct);
            var total = await _follows.CountFollowingAsync(userId, ct);

            var items = new List<UserBriefDto>(ids.Count);
            foreach (var id in ids)
            {
                var u = await _users.GetByIdAsync(id, ct);
                if (u != null) items.Add(new UserBriefDto(u.Id, u.UserName, u.DisplayName));
            }
            return (items, total);
        }
        public async Task<IReadOnlyList<Guid>> GetFollowingIdsAsync(Guid me, int page, int size, CancellationToken ct = default)
        {
            var (skip, take) = Paginate(page, size);
            return await _follows.GetFollowingIdsAsync(me, skip, take, ct);
        }
        private static (int skip, int take) Paginate(int page, int size)
        {
            page = page <= 0 ? 1 : page;
            size = size is <= 0 or > 100 ? 20 : size;
            return ((page - 1) * size, size);
        }

    }
}
