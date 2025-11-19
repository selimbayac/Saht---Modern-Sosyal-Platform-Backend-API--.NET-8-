using Saht.Application.Abstractions;
using Saht.Application.Reactions;
using Saht.Application.SocialConnections;
using Saht.Domain.Notifications;
using Saht.Domain.Posts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Saht.Application.Posts.PostCommands;

namespace Saht.Application.Posts
{
    public sealed class PostService : IPostService
    {
        private readonly IPostRepository _posts;
        private readonly IUserRepository _users;
        private readonly IReactionService _reactions;
        private readonly IFollowRepository _follows;
        private readonly INotificationRepository _notifs;
        private readonly IPrivacyService _privacy;
        public PostService(IPostRepository posts, IUserRepository users, IReactionService reactionservice, IFollowRepository follows,
    INotificationRepository notifs, IPrivacyService privacy)
        {
            _posts = posts; _users = users;
            _reactions = reactionservice;
            _follows = follows;
            _notifs = notifs;
            _privacy = privacy;
        }

        public async Task<PostDto> CreateAsync(Guid authorId, CreatePostCommand cmd, CancellationToken ct)
        {
            EnsureBody(cmd.Body);

            var p = Post.CreateNormal(authorId, cmd.Body);
            await _posts.AddAsync(p, ct);
            await _posts.SaveChangesAsync(ct);

            // 1. TAKİPÇİ BİLDİRİMİ (NotificationType.FolloweeNewPost = 60)
            var followerIds = await _follows.GetFollowerIdsOfAsync(authorId, ct);

            if (followerIds.Count > 0)
            {
                var payload = JsonSerializer.Serialize(new { actorId = authorId, postId = p.Id });

                foreach (var followerId in followerIds)
                {
                    if (followerId == authorId) continue;

                    var notif = Notification.Create(
                        followerId,
                        NotificationType.FolloweeNewPost,
                        payload
                    );

                    await _notifs.AddAsync(notif, ct);
                }
            }

            // 2. YENİ EKLENEN KISIM: ETİKETLEME BİLDİRİMİ (NotificationType.MentionInPost = 5)
            // Bu kısım, takipçi bildiriminin kapsamına dahil edilmediği için, takipçi kontrolünün dışına taşınmalıdır.
            await SendMentionNotificationsAsync(
                p.Body!, authorId, p.Id, NotificationType.MentionInPost, ct);

            // Takipçi ve Etiketleme bildirimlerini tek seferde kaydet
            await _notifs.SaveChangesAsync(ct);

            return await MapAsync(p, authorId, ct);
        }

        public async Task<PostDto> ReplyAsync(Guid authorId, ReplyCommand cmd, CancellationToken ct = default)
        {
            EnsureBody(cmd.Body);
            var parent = await _posts.GetByIdAsync(cmd.ParentPostId, ct) ?? throw new KeyNotFoundException("Parent yok");
            var p = Post.CreateReply(authorId, parent.Id, cmd.Body);
            await _posts.AddAsync(p, ct);
            await _posts.SaveChangesAsync(ct);
            // NOT: Reply (Cevap) da bir Post olduğu için burada da Mention kontrolü yapılabilir
            // Eğer Reply metni içinde etiket varsa MentionInPost bildirimi gönderilir.
            await SendMentionNotificationsAsync(
                cmd.Body, authorId, p.Id, NotificationType.MentionInPost, ct);
            await _notifs.SaveChangesAsync(ct);

            return await MapAsync(p, authorId, ct);
        }

        public async Task<PostDto> QuoteAsync(Guid authorId, QuoteCommand cmd, CancellationToken ct)
        {
            EnsureBody(cmd.Body);
            _ = await _posts.GetByIdAsync(cmd.ParentPostId, ct) ?? throw new KeyNotFoundException("Parent yok");
            var p = Post.CreateQuote(authorId, cmd.ParentPostId, cmd.Body);
            await _posts.AddAsync(p, ct);
            await _posts.SaveChangesAsync(ct);
            // NOT: Quote da bir Post olduğu için burada da Mention kontrolü yapılabilir
            await SendMentionNotificationsAsync(
                cmd.Body, authorId, p.Id, NotificationType.MentionInPost, ct);
            await _notifs.SaveChangesAsync(ct);

            return await MapAsync(p, authorId, ct);
        }


        public async Task<PostDto> RepostAsync(Guid authorId, RepostCommand cmd, CancellationToken ct)
        {
            _ = await _posts.GetByIdAsync(cmd.ParentPostId, ct) ?? throw new KeyNotFoundException("Parent yok");
            var p = Post.CreateRepost(authorId, cmd.ParentPostId);
            await _posts.AddAsync(p, ct);
            await _posts.SaveChangesAsync(ct);
            return await MapAsync(p, authorId, ct);
        }

        public async Task<PostDto?> GetByIdAsync(long id, Guid viewerId, CancellationToken ct = default)
        {
            var p = await _posts.GetByIdAsync(id, ct);
            return p is null ? null : await MapAsync(p, viewerId, ct);
        }

        public async Task<PagedList<PostDto>> GetFeedAsync(Guid viewerId, int page, int size, CancellationToken ct)
        {
            var (skip, take) = Paginate(page, size);

     
            var (list, totalCount) = await _posts.GetFeedAsync(viewerId, skip, take, ct);

            var postDtos = await MapListAsync(list, viewerId, ct);

            return new PagedList<PostDto>(postDtos, totalCount, page, size);
        }

        public async Task<PagedList<PostDto>> GetUserTimelineAsync(Guid targetUserId, Guid viewerId, int page, int size, CancellationToken ct)
        {
            var (skip, take) = Paginate(page, size);

            // 🚨 Repository metot ismi ve dönüş tipi önceki cevaplara göre düzeltildi
            var (list, totalCount) = await _posts.GetUserPostsAsync(targetUserId, skip, take, ct);

            var postDtos = await MapListAsync(list, viewerId, ct);

            return new PagedList<PostDto>(postDtos, totalCount, page, size);
        }
        // PostService.cs
        public async Task<PagedList<PostDto>> GetPublicFeedAsync(Guid viewerId, int page, int size, CancellationToken ct)
        {
            var (skip, take) = Paginate(page, size);

            IReadOnlyList<Guid> blockedIds; // 👈 Değişkenin tipini netleştiriyoruz

            if (viewerId != Guid.Empty)
            {
                // Zaten IReadOnlyList<Guid> döner
                blockedIds = await _privacy.GetBlockedIdsAsync(viewerId, ct);
            }
            else
            {
                // 🚨 DÜZELTME: IEnumerable<Guid>'ı IReadOnlyList<Guid>'a dönüştürmek için ToList() kullanıldı
                blockedIds = Enumerable.Empty<Guid>().ToList();
            }

            // Tüm sorgulama ve sayfalama sorumluluğu Repository'ye devredildi.
            var (list, totalCount) = await _posts.GetPublicPostsAsync(
                 viewerId,
                 blockedIds, // Artık IReadOnlyList<Guid> tipinde
                 skip,
                 take,
                 ct);

            var postDtos = await MapListAsync(list, viewerId, ct);

            return new PagedList<PostDto>(postDtos, totalCount, page, size);
        }


        public async Task<PostDto> EditAsync(Guid authorId, EditPostCommand cmd, CancellationToken ct = default)
        {
            EnsureBody(cmd.Body);
            if (!await _posts.IsOwnerAsync(cmd.PostId, authorId, ct))
            {
                // Yetki yoksa fırlat (401/403 hatasına dönüşür)
                throw new UnauthorizedAccessException("Sadece yazar düzenleyebilir.");
            }
            var p = await _posts.GetByIdAsync(cmd.PostId, ct) ??
             throw new KeyNotFoundException("Düzenlenecek gönderi bulunamadı.");
            p.Edit(cmd.Body);
            await _posts.SaveChangesAsync(ct);
            return await MapAsync(p, authorId, ct);
        }

        public async Task DeleteAsync(Guid authorId, long postId, bool isModerator = false, CancellationToken ct = default)
        {
            if (!isModerator)
            {
                var owns = await _posts.IsOwnerAsync(postId, authorId, ct);
                if (!owns) throw new UnauthorizedAccessException("Sadece yazar silebilir");
            }
            await _posts.SoftDeleteAsync(postId, ct);
            await _posts.SaveChangesAsync(ct);
        }

        // helpers
        private static void EnsureBody(string? body)
        {
            if (string.IsNullOrWhiteSpace(body)) throw new ArgumentException("Metin boş olamaz");
            if (body.Length > 4000) throw new ArgumentException("Metin 4000 karakteri aşmamalı");
        }

        private async Task<PostDto> MapAsync(Post p, Guid viewerId, CancellationToken ct)
        {
            var author = await _users.GetByIdAsync(p.AuthorId, ct) ?? throw new InvalidOperationException("Yazar bulunamadı");
            var summary = await _reactions.GetPostSummaryAsync(viewerId == Guid.Empty ? Guid.Empty : viewerId, p.Id, ct);

            return new PostDto(
                p.Id, p.Type.ToString(), p.Body, p.ParentPostId,
                author.Id, author.UserName, author.DisplayName,
                p.CreatedAt, p.EditedAt,
                summary
            );
        }


        private async Task<IReadOnlyList<PostDto>> MapListAsync(IEnumerable<Post> posts, Guid viewerId, CancellationToken ct)
        {
            var list = new List<PostDto>();
            foreach (var p in posts)
                list.Add(await MapAsync(p, viewerId, ct));
            return list;
        }
        private static (int skip, int take) Paginate(int page, int size)
        {
            page = page <= 0 ? 1 : page;
            size = size is <= 0 or > 50 ? 20 : size;
            return ((page - 1) * size, size);
        }
        private async Task SendMentionNotificationsAsync(string body, Guid actorId, long contentId, NotificationType type, CancellationToken ct)
        {
            var usernames = GetMentionsFromBody(body);

            if (!usernames.Any()) return;

            // Kullanıcı adlarını gerçek kullanıcı ID'lerine çevir
            // (IUserRepository'de GetUserIdsByUsernamesAsync metodunun var olduğunu varsayıyoruz)
            var mentionedUserIds = await _users.GetUserIdsByUsernamesAsync(usernames, ct);

            foreach (var mentionedId in mentionedUserIds)
            {
                // Kendini etiketleyenlere bildirim gönderme
                if (mentionedId == actorId) continue;

                var payload = JsonSerializer.Serialize(new { actorId, contentId });
                // NotificationType.MentionInPost = 5'i veya MentionInComment = 6'yı kullanır.
                var notif = Notification.Create(mentionedId, type, payload);
                await _notifs.AddAsync(notif, ct);
            }
            // NOT: SaveChangesAsync burada çağrılmıyor, çünkü çağrılan metodun sonunda (CreateAsync, ReplyAsync, QuoteAsync) toplu olarak kaydediliyor.
        }

        private static IReadOnlyList<string> GetMentionsFromBody(string body)
        {
            var mentions = new List<string>();

            // Kelimeleri ayır
            var words = body.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                if (word.StartsWith('@') && word.Length > 1)
                {
                    // @ işaretinden sonra gelen kısmı al ve noktalama/boşlukları temizle.
                    var username = new string(word.Skip(1).TakeWhile(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
                    if (!string.IsNullOrWhiteSpace(username))
                    {
                        mentions.Add(username);
                    }
                }
            }
            return mentions.Distinct().ToList();
        }
    }
 }
