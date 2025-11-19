using Saht.Application.Abstractions;
using Saht.Domain.Common;
using Saht.Domain.Notifications;
using Saht.Domain.Reactions;
using Saht.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Saht.Application.Reactions
{
    public sealed class ReactionService : IReactionService
    {
        private readonly IReactionRepository _reactions;
        private readonly IPostRepository _posts;
        private readonly ICommentRepository _comments;
        private readonly IUserRepository _users;
        private readonly IBlogPostRepository _blogs;
        private readonly INotificationRepository _notifs;

        public ReactionService(IReactionRepository reactions, IPostRepository posts, ICommentRepository comments, IUserRepository users, IBlogPostRepository blogs, INotificationRepository notifs)
        {
            _reactions = reactions; _posts = posts;
            _comments = comments;
            _users = users;
            _blogs = blogs;
            _notifs = notifs;
        }

        public async Task ReactToPostAsync(Guid userId, ReactCommand cmd, CancellationToken ct = default)
        {
            if (cmd.Value is not (1 or -1)) throw new ArgumentException("Değer +1/-1 olmalı");

            var post = await _posts.GetByIdAsync(cmd.TargetId, ct) ?? throw new KeyNotFoundException("Post yok");

            var existing = await _reactions.GetAsync(ContentType.Post, cmd.TargetId, userId, ct);

            // Reaksiyon ilk kez mi veriliyor?
            bool isNewReaction = existing is null;
            // Mevcut reaksiyon değeri değişiyor mu? (Örn: Like -> Dislike)
            bool valueChanged = existing is not null && existing.Value != cmd.Value;

            if (existing is null)
                await _reactions.UpsertAsync(Reaction.Give(userId, ContentType.Post, cmd.TargetId, cmd.Value), ct);
            else
            {
                existing.Change(cmd.Value);
                await _reactions.UpsertAsync(existing, ct);
            }

            await _reactions.SaveChangesAsync(ct);

            // 3. BİLDİRİM MANTIĞI: Sadece reaksiyon ilk kez verildiyse veya değeri değiştiyse bildirim gönder
            if ((isNewReaction || valueChanged) && post.AuthorId != userId)
            {
                var payload = JsonSerializer.Serialize(new { actorId = userId, postId = cmd.TargetId });
                // Note: Dislike (value = -1) için bildirim göndermek mantıklı olmayabilir, 
                // bu yüzden NotificationType.PostLiked kullanılıyorsa, burada sadece cmd.Value == 1 iken bildirim göndermeyi düşünebilirsiniz.
                // Şimdilik geleneksel "Post Beğenildi" (Liked) bildirimini koruyoruz.
                var notif = Notification.Create(post.AuthorId, NotificationType.PostLiked, payload);
                await _notifs.AddAsync(notif, ct);
                await _notifs.SaveChangesAsync(ct); // Bildirimleri kaydet
            }
        }

        public async Task RemoveFromPostAsync(Guid userId, long postId, CancellationToken ct = default)
        {
            _ = await _posts.GetByIdAsync(postId, ct) ?? throw new KeyNotFoundException("Post yok");
            await _reactions.RemoveAsync(ContentType.Post, postId, userId, ct);
            await _reactions.SaveChangesAsync(ct);
        }

        public async Task<ReactionSummaryDto> GetPostSummaryAsync(Guid userIdOrEmpty, long postId, CancellationToken ct = default)
        {
            _ = await _posts.GetByIdAsync(postId, ct) ?? throw new KeyNotFoundException("Post yok");
            var (likes, dislikes) = await _reactions.CountAsync(ContentType.Post, postId, ct);

            int? my = null;
            if (userIdOrEmpty != Guid.Empty)
            {
                var me = await _reactions.GetAsync(ContentType.Post, postId, userIdOrEmpty, ct);
                if (me != null) my = me.Value;
            }
            return new ReactionSummaryDto(likes, dislikes, likes - dislikes, my);
        }

        public async Task<(IReadOnlyList<ReactionUserDto> Items, int Total)> GetPostReactorsAsync(long postId, int? value, int page, int size, CancellationToken ct = default)
        {
            _ = await _posts.GetByIdAsync(postId, ct) ?? throw new KeyNotFoundException("Post yok");
            page = Math.Max(1, page); size = Math.Clamp(size, 1, 100);
            var skip = (page - 1) * size;

            var (rows, total) = await _reactions.GetReactorsAsync(ContentType.Post, postId, value, skip, size, ct);

            var list = new List<ReactionUserDto>(rows.Count);
            foreach (var r in rows)
            {
                var u = await _users.GetByIdAsync(r.userId, ct) ?? throw new InvalidOperationException("Kullanıcı yok");
                list.Add(new ReactionUserDto(u.Id, u.UserName, u.DisplayName, r.value, r.at));
            }
            return (list, total);
        }

        // ==== COMMENT ====

        public async Task ReactToCommentAsync(Guid userId, long commentId, int value, CancellationToken ct = default)
        {
            if (value is not (1 or -1)) throw new ArgumentException("Değer +1/-1 olmalı");

            var comment = await _comments.GetByIdAsync(commentId, ct) ?? throw new KeyNotFoundException("Yorum yok");

            var existing = await _reactions.GetAsync(ContentType.Comment, commentId, userId, ct);

            bool isNewReaction = existing is null;
            bool valueChanged = existing is not null && existing.Value != value;

            if (existing is null)
                await _reactions.UpsertAsync(Reaction.Give(userId, ContentType.Comment, commentId, value), ct);
            else { existing.Change(value); await _reactions.UpsertAsync(existing, ct); }

            await _reactions.SaveChangesAsync(ct);

            // BİLDİRİM MANTIĞI: Sadece reaksiyon ilk kez verildiyse veya değeri değiştiyse bildirim gönder
            if ((isNewReaction || valueChanged) && comment.UserId != userId)
            {
                var payload = JsonSerializer.Serialize(new { actorId = userId, commentId = commentId });
                var notif = Notification.Create(comment.UserId, NotificationType.CommentLiked, payload);
                await _notifs.AddAsync(notif, ct);
                await _notifs.SaveChangesAsync(ct);
            }
        }

        public async Task RemoveFromCommentAsync(Guid userId, long commentId, CancellationToken ct = default)
        {
            _ = await _comments.GetByIdAsync(commentId, ct) ?? throw new KeyNotFoundException("Yorum yok");
            await _reactions.RemoveAsync(ContentType.Comment, commentId, userId, ct);
            await _reactions.SaveChangesAsync(ct);
        }

        public async Task<ReactionSummaryDto> GetCommentSummaryAsync(Guid viewerOrEmpty, long commentId, CancellationToken ct = default)
        {
            _ = await _comments.GetByIdAsync(commentId, ct) ?? throw new KeyNotFoundException("Yorum yok");
            var (likes, dislikes) = await _reactions.CountAsync(ContentType.Comment, commentId, ct);

            int? my = null;
            if (viewerOrEmpty != Guid.Empty)
            {
                var me = await _reactions.GetAsync(ContentType.Comment, commentId, viewerOrEmpty, ct);
                if (me != null) my = me.Value;
            }
            return new ReactionSummaryDto(likes, dislikes, likes - dislikes, my);
        }

        public async Task<(IReadOnlyList<ReactionUserDto> Items, int Total)> GetCommentReactorsAsync(long commentId, int? value, int page, int size, CancellationToken ct = default)
        {
            _ = await _comments.GetByIdAsync(commentId, ct) ?? throw new KeyNotFoundException("Yorum yok");
            page = Math.Max(1, page); size = Math.Clamp(size, 1, 100);
            var skip = (page - 1) * size;

            var (rows, total) = await _reactions.GetReactorsAsync(ContentType.Comment, commentId, value, skip, size, ct);

            var list = new List<ReactionUserDto>(rows.Count);
            foreach (var r in rows)
            {
                var u = await _users.GetByIdAsync(r.userId, ct) ?? throw new InvalidOperationException("Kullanıcı yok");
                list.Add(new ReactionUserDto(u.Id, u.UserName, u.DisplayName, r.value, r.at));
            }
            return (list, total);
        }
        // ==== BLOGPOST ====
        public async Task ReactToBlogPostAsync(Guid userId, long blogPostId, int value, CancellationToken ct = default)
        {
            if (value is not (1 or -1)) throw new ArgumentException("Değer +1/-1 olmalı");

            var blog = await _blogs.GetByIdAsync(blogPostId, ct) ?? throw new KeyNotFoundException("Blog yazısı yok");

            var existing = await _reactions.GetAsync(ContentType.BlogPost, blogPostId, userId, ct);

            bool isNewReaction = existing is null;
            bool valueChanged = existing is not null && existing.Value != value;

            if (existing is null)
                await _reactions.UpsertAsync(Reaction.Give(userId, ContentType.BlogPost, blogPostId, value), ct);
            else { existing.Change(value); await _reactions.UpsertAsync(existing, ct); }

            await _reactions.SaveChangesAsync(ct);

            // BİLDİRİM MANTIĞI: Sadece reaksiyon ilk kez verildiyse veya değeri değiştiyse bildirim gönder
            if ((isNewReaction || valueChanged) && blog.AuthorId != userId)
            {
                var payload = JsonSerializer.Serialize(new { actorId = userId, blogPostId = blogPostId });
                var notif = Notification.Create(blog.AuthorId, NotificationType.BlogPostLiked, payload);
                await _notifs.AddAsync(notif, ct);
                await _notifs.SaveChangesAsync(ct);
            }
        }

        public async Task RemoveFromBlogPostAsync(Guid userId, long blogPostId, CancellationToken ct = default)
        {
            _ = await _blogs.GetByIdAsync(blogPostId, ct) ?? throw new KeyNotFoundException("Blog yazısı yok");
            await _reactions.RemoveAsync(ContentType.BlogPost, blogPostId, userId, ct);
            await _reactions.SaveChangesAsync(ct);
        }

        public async Task<ReactionSummaryDto> GetBlogPostSummaryAsync(Guid viewerOrEmpty, long blogPostId, CancellationToken ct = default)
        {
            _ = await _blogs.GetByIdAsync(blogPostId, ct) ?? throw new KeyNotFoundException("Blog yazısı yok");
            var (likes, dislikes) = await _reactions.CountAsync(ContentType.BlogPost, blogPostId, ct);

            int? my = null;
            if (viewerOrEmpty != Guid.Empty)
            {
                var me = await _reactions.GetAsync(ContentType.BlogPost, blogPostId, viewerOrEmpty, ct);
                if (me != null) my = me.Value;
            }
            return new ReactionSummaryDto(likes, dislikes, likes - dislikes, my);
        }

        public async Task<(IReadOnlyList<ReactionUserDto> Items, int Total)> GetBlogPostReactorsAsync(long blogPostId, int? value, int page, int size, CancellationToken ct = default)
        {
            _ = await _blogs.GetByIdAsync(blogPostId, ct) ?? throw new KeyNotFoundException("Blog yazısı yok");
            page = Math.Max(1, page); size = Math.Clamp(size, 1, 100);
            var skip = (page - 1) * size;

            var (rows, total) = await _reactions.GetReactorsAsync(ContentType.BlogPost, blogPostId, value, skip, size, ct);

            var list = new List<ReactionUserDto>(rows.Count);
            foreach (var r in rows)
            {
                var u = await _users.GetByIdAsync(r.userId, ct) ?? throw new InvalidOperationException("Kullanıcı yok");
                list.Add(new ReactionUserDto(u.Id, u.UserName, u.DisplayName, r.value, r.at));
            }
            return (list, total);
        }
      
    }
}