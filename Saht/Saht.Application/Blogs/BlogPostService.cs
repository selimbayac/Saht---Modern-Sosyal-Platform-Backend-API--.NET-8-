using Saht.Application.Abstractions;
using Saht.Application.Comments;
using Saht.Domain.Blogs;
using Saht.Domain.Common;
using Saht.Domain.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Saht.Application.Blogs
{
    public sealed class BlogPostService : IBlogPostService
    {
        private readonly IBlogPostRepository _blogs;
        private readonly IUserRepository _users;
        private readonly IFollowRepository _follows;
        private readonly INotificationRepository _notifs;
        private readonly ICommentService _comments;

        public BlogPostService(
            IBlogPostRepository blogs,
            IUserRepository users,
            IFollowRepository follows,
            INotificationRepository notifs,
            ICommentService comments)
        {
            _blogs = blogs; _users = users; _follows = follows; _notifs = notifs; _comments = comments;
        }

        public async Task<BlogPostDto> CreateAsync(Guid authorId, CreateBlogPostCommand cmd, CancellationToken ct = default)
        {
            var b = BlogPost.Create(authorId, cmd.Title, cmd.Body);
            await _blogs.AddAsync(b, ct);
            await _blogs.SaveChangesAsync(ct);

            // 1. Takipçilere bildirim
            var followerIds = await _follows.GetFollowerIdsOfAsync(authorId, ct);
            if (followerIds.Count > 0)
            {
                var payload = JsonSerializer.Serialize(new { actorId = authorId, blogPostId = b.Id });
                foreach (var fid in followerIds)
                {
                    if (fid == authorId) continue;
                    var n = Notification.Create(fid, NotificationType.FolloweeNewBlogPost, payload);
                    await _notifs.AddAsync(n, ct);
                }
            }

            // 2. YENİ EKLENEN: Blog içeriğindeki etiketlemeler (Mention in Post)
            // Blog içeriği metninde (@kullanıcıadı) geçenler için bildirim gönder.
            await SendMentionNotificationsAsync(
                b.Body, authorId, b.Id, NotificationType.MentionInPost, ct);

            // Tüm bildirimleri kaydet (Takipçiler + Etiketler)
            await _notifs.SaveChangesAsync(ct);

            return await MapAsync(b, ct);
        }

        public async Task<BlogPostDto?> GetByIdAsync(long id, CancellationToken ct = default)
        {
            var b = await _blogs.GetByIdAsync(id, ct);
            return b is null ? null : await MapAsync(b, ct);
        }

        public async Task<IReadOnlyList<BlogPostDto>> ListByUserAsync(Guid userId, int page, int size, CancellationToken ct = default)
        {
            var (skip, take) = Paginate(page, size);
            var list = await _blogs.ListByUserAsync(userId, skip, take, ct);
            return await MapListAsync(list, ct);
        }

        public async Task<IReadOnlyList<BlogPostDto>> ListRecentAsync(int page, int size, CancellationToken ct = default)
        {
            var (skip, take) = Paginate(page, size);
            var list = await _blogs.ListRecentAsync(skip, take, ct);
            return await MapListAsync(list, ct);
        }

        public async Task<BlogPostDto> EditAsync(Guid authorId, EditBlogPostCommand cmd, CancellationToken ct = default)
        {
            var b = await _blogs.GetByIdAsync(cmd.Id, ct) ?? throw new KeyNotFoundException("Blog bulunamadı");
            var owns = await _blogs.IsOwnerAsync(b.Id, authorId, ct);
            if (!owns) throw new UnauthorizedAccessException("Sadece sahibi düzenler");

            b.Edit(cmd.Title, cmd.Body);
            await _blogs.SaveChangesAsync(ct);
            return await MapAsync(b, ct);
        }

        public async Task DeleteAsync(Guid authorId, long blogId, bool isModerator, CancellationToken ct = default)
        {
            if (!isModerator)
            {
                var owns = await _blogs.IsOwnerAsync(blogId, authorId, ct);
                if (!owns) throw new UnauthorizedAccessException("Sadece sahibi silebilir");
            }
            await _blogs.SoftDeleteAsync(blogId, ct);
            await _blogs.SaveChangesAsync(ct);
        }

        // --- Comments (BlogPost) ---
        public Task<Paged<CommentDto>> ListCommentsAsync(long blogId, int page, int size, Guid viewerOrEmpty, CancellationToken ct = default)
            => _comments.ListForTargetAsync(ContentType.BlogPost, blogId, page, size, viewerOrEmpty, ct);

        // --- BLOG YORUMU OLUŞTURMA ---
        public async Task<CommentDto> CreateCommentAsync(Guid authorId, long blogId, string body, CancellationToken ct = default)
        {
            // CommentService’e blog için bir metot ekleyelim:
            // Bu metot, yorumu oluşturur ve kaydeder.
            var dto = await _comments.CreateOnTargetAsync(authorId, ContentType.BlogPost, blogId, body, ct);

            // Blog yazarına bildirim
            var blog = await _blogs.GetByIdAsync(blogId, ct) ?? throw new KeyNotFoundException("Blog yok");

            // 1. Blog Yazarını Bilgilendirme (Yorum yapıldı)
            if (blog.AuthorId != authorId)
            {
                var payload = JsonSerializer.Serialize(new { blogPostId = blogId, commentId = dto.Id, fromUserId = authorId });
                var n = Notification.Create(blog.AuthorId, NotificationType.BlogPostCommented, payload);
                await _notifs.AddAsync(n, ct);
            }

            // 2. YORUM İÇİNDEKİ ETİKETLEMELER: CommentService'te zaten yapıldığı varsayılır
            // Ancak CommentService'in SaveChangesAsync() metodunu sadece kendi içinde çağırması iyi bir pratik. 
            // Burada ek bir iş yapılmasına gerek kalmadı, çünkü CommentService'te (önceki cevabımızda) bu mantığı hallettik.

            // Yazar bildirimlerini kaydet
            await _notifs.SaveChangesAsync(ct);

            return dto;
        }

        // helpers
        private async Task<BlogPostDto> MapAsync(BlogPost b, CancellationToken ct)
        {
            var u = await _users.GetByIdAsync(b.AuthorId, ct) ?? throw new InvalidOperationException("Yazar yok");
            return new BlogPostDto(b.Id, u.Id, u.UserName, u.DisplayName, b.Title, b.Body, b.CreatedAt, b.EditedAt);
        }

        private async Task<IReadOnlyList<BlogPostDto>> MapListAsync(IEnumerable<BlogPost> list, CancellationToken ct)
        {
            var res = new List<BlogPostDto>();
            foreach (var b in list) res.Add(await MapAsync(b, ct));
            return res;
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

            // _users.GetUserIdsByUsernamesAsync metodu IUserRepository'de tanımlı olmalıdır.
            var mentionedUserIds = await _users.GetUserIdsByUsernamesAsync(usernames, ct);

            foreach (var mentionedId in mentionedUserIds)
            {
                // Etiketleyen kişi kendini etiketlemişse bildirim gönderme.
                if (mentionedId == actorId) continue;

                var payload = JsonSerializer.Serialize(new { actorId, contentId });
                var notif = Notification.Create(mentionedId, type, payload);
                await _notifs.AddAsync(notif, ct);
            }
        }

        private static IReadOnlyList<string> GetMentionsFromBody(string body)
        {
            var mentions = new List<string>();
            // Metni boşluklara göre ayır
            var words = body.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                // @ ile başlıyorsa ve @'dan sonra bir şey varsa
                if (word.StartsWith('@') && word.Length > 1)
                {
                    // Kullanıcı adını (@ hariç) al
                    var username = new string(word.Skip(1).TakeWhile(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
                    if (!string.IsNullOrWhiteSpace(username))
                    {
                        mentions.Add(username);
                    }
                }
            }
            // Aynı kişiyi birden çok kez etiketlemişse sadece bir kez al
            return mentions.Distinct().ToList();
        }
    }
}
