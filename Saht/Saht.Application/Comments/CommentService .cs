using Saht.Application.Abstractions;
using Saht.Application.Reactions;
using Saht.Domain.Comments;
using Saht.Domain.Common;
using Saht.Domain.Notifications;
using Saht.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Saht.Application.Comments
{
    public sealed class CommentService : ICommentService
    {
        private readonly ICommentRepository _comments;
        private readonly IPostRepository _posts;
        private readonly IUserRepository _users;
        private readonly IReactionService _reactions;
        private readonly IBlogPostRepository _blogs;
        private readonly INotificationRepository _notifs;

        public CommentService(ICommentRepository comments, IPostRepository posts, IUserRepository users, IReactionService reactions, IBlogPostRepository blogs, INotificationRepository notifs)
        {
            _comments = comments;
            _posts = posts;
            _users = users;
            _reactions = reactions;
            _blogs = blogs;
            _notifs = notifs;
        }

        public async Task<CommentDto> CreateOnPostAsync(Guid userId, CreateOnPostCommand cmd, CancellationToken ct = default)
        {
            EnsureBody(cmd.Body);
            _ = await _posts.GetByIdAsync(cmd.PostId, ct) ?? throw new KeyNotFoundException("Post yok");

            var c = Comment.CreateRoot(ContentType.Post, cmd.PostId, userId, cmd.Body);
            await _comments.AddAsync(c, ct);
            await _comments.SaveChangesAsync(ct);

            // YENİ EKLENEN: Etiketleme (Mention) Bildirimi
            await SendMentionNotificationsAsync(
                cmd.Body, userId, c.Id, NotificationType.MentionInComment, ct);
            await _notifs.SaveChangesAsync(ct); // Bildirimleri kaydet

            return await MapAsync(c, viewerOrEmpty: Guid.Empty, ct);
        }

        public async Task<CommentDto> ReplyAsync(Guid userId, ReplyCommentCommand cmd, CancellationToken ct = default)
        {
            EnsureBody(cmd.Body);
            var parent = await _comments.GetByIdAsync(cmd.ParentCommentId, ct) ?? throw new KeyNotFoundException("Yorum yok");

            var c = Comment.CreateReply(parent.TargetType, parent.TargetId, userId, parent.Id, cmd.Body);
            await _comments.AddAsync(c, ct);
            await _comments.SaveChangesAsync(ct);

            // YENİ EKLENEN: Etiketleme (Mention) Bildirimi
            await SendMentionNotificationsAsync(
                cmd.Body, userId, c.Id, NotificationType.MentionInComment, ct);
            await _notifs.SaveChangesAsync(ct); // Bildirimleri kaydet

            return await MapAsync(c, viewerOrEmpty: Guid.Empty, ct);
        }

        public async Task<CommentDto> EditAsync(Guid userId, EditCommentCommand cmd, CancellationToken ct = default)
        {
            EnsureBody(cmd.Body);
            var c = await _comments.GetByIdAsync(cmd.CommentId, ct) ?? throw new KeyNotFoundException("Yorum yok");

            var owner = await _comments.IsOwnerAsync(c.Id, userId, ct);
            if (!owner) throw new UnauthorizedAccessException("Sadece sahibi düzenler.");

            c.Edit(cmd.Body);
            await _comments.SaveChangesAsync(ct);

            return await MapAsync(c, viewerOrEmpty: Guid.Empty, ct);
        }

        public async Task DeleteAsync(Guid userId, long commentId, bool isModerator = false, CancellationToken ct = default)
        {
            if (!isModerator)
            {
                var owner = await _comments.IsOwnerAsync(commentId, userId, ct);
                if (!owner) throw new UnauthorizedAccessException("Sadece sahibi silebilir.");
            }

            await _comments.SoftDeleteAsync(commentId, ct);
            await _comments.SaveChangesAsync(ct);
        }

        public async Task<CommentDto?> GetByIdAsync(long id, Guid viewerOrEmpty, CancellationToken ct = default)
        {
            var c = await _comments.GetByIdAsync(id, ct);
            return c is null ? null : await MapAsync(c, viewerOrEmpty, ct);
        }

        public async Task<Paged<CommentDto>> ListForTargetAsync(
      ContentType targetType,
      long targetId,
      int page,
      int size,
      Guid viewerOrEmpty,
      CancellationToken ct = default)
        {
            var (skip, take) = Paginate(page, size);

            // 1. Yorumları ve toplam sayıyı çek (2 Sorgu)
            var total = await _comments.CountForTargetAsync(targetType, targetId, ct);
            var items = await _comments.ListForTargetAsync(targetType, targetId, skip, take, ct);

            if (items.Count == 0)
            {
                return new Paged<CommentDto>(total, new List<CommentDto>());
            }

            // 2. ADIM: Tüm benzersiz yazar ID'lerini topla
            var authorIds = items.Select(c => c.UserId).Distinct().ToList();

            // 3. ADIM: Tüm yazarları tek sorguyla çek (1 Sorgu - Batching)
            // Bu, N+1 sorununu çözen kritik adımdır.
            var authorsMap = await _users.GetUsersByIdsAsync(authorIds, ct);

            var list = new List<CommentDto>(items.Count);

            // 4. ADIM: DTO'lara haritalama
            foreach (var c in items)
            {
                // Reaksiyon özetini tek tek çekiyoruz (Eğer Reaction servisinde de toplu çekim yoksa)
                ReactionSummaryDto? summary = null;
                try
                {
                    summary = await _reactions.GetCommentSummaryAsync(viewerOrEmpty, c.Id, ct);
                }
                catch
                {
                    // Hata durumunda reaksiyon özeti null kalır.
                }
                list.Add(MapToDto(c, authorsMap, viewerOrEmpty, summary));
            }

            return new Paged<CommentDto>(total, list);
        }

        // ------------- helpers -------------

        private async Task<CommentDto> MapAsync(Comment c, Guid viewerOrEmpty, CancellationToken ct )
        {
            
            var author = await _users.GetByIdAsync(c.UserId, ct) ?? throw new InvalidOperationException("Kullanıcı yok");
            // Reaction summary (viewer varsa "my" dolacak)
            ReactionSummaryDto? summary = null;
            try
            {
                summary = await _reactions.GetCommentSummaryAsync(viewerOrEmpty, c.Id, ct);
            }
            catch
            {
                // Reactions servisi hazır değilse ya da exception atarsa null bırak.
            }

            return new CommentDto(
                c.Id,
                c.TargetType.ToString(),       
                c.TargetId,
                (int)c.TargetType,
                author.Id,
                author.UserName,
                author.DisplayName,
                c.Body,
                c.ParentCommentId,
                c.CreatedAt,
                c.EditedAt,
                c.IsDeleted,
                summary
            );
        }
        private CommentDto MapToDto(
    Comment c,
    IReadOnlyDictionary<Guid, User> authors,
    Guid viewerOrEmpty,
    ReactionSummaryDto? summary)
        {
            // Yazar bilgisini Map'ten (sözlükten) O(1) hızında çekiyoruz, DB sorgusu yok.
            var author = authors.GetValueOrDefault(c.UserId)
                         ?? throw new InvalidOperationException($"Kullanıcı bulunamadı: {c.UserId}");

            return new CommentDto(
                c.Id,
                c.TargetType.ToString(),
                c.TargetId,
                (int)c.TargetType,
                author.Id,              // DTO'ya kullanıcı ID'sini basıyoruz
                author.UserName,        // DTO'ya UserName basıyoruz
                author.DisplayName,     // DTO'ya DisplayName basıyoruz
                c.Body,
                c.ParentCommentId,
                c.CreatedAt,
                c.EditedAt,
                c.IsDeleted,
                summary
            );
        }
        private static void EnsureBody(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) throw new ArgumentException("Metin boş olamaz.");
            if (body.Length > 4000) throw new ArgumentException("Metin 4000’i aşmamalı.");
        }

        private static (int skip, int take) Paginate(int page, int size)
        {
            page = page <= 0 ? 1 : page;
            size = size is <= 0 or > 50 ? 20 : size;
            return ((page - 1) * size, size);
        }
        public async Task<CommentDto> CreateOnTargetAsync(Guid userId, ContentType targetType, long targetId, string body, CancellationToken ct = default)
        {
            EnsureBody(body);
            // hedef doğrulama
            switch (targetType)
            {
                case ContentType.Post:
                    _ = await _posts.GetByIdAsync(targetId, ct) ?? throw new KeyNotFoundException("Post yok");
                    break;
                case ContentType.BlogPost:
                    _ = await _blogs.GetByIdAsync(targetId, ct) ?? throw new KeyNotFoundException("Blog yok");
                    break;
                default:
                    throw new NotSupportedException("Desteklenmeyen hedef");
            }

            var c = Comment.CreateRoot(targetType, targetId, userId, body);
            await _comments.AddAsync(c, ct);
            await _comments.SaveChangesAsync(ct);
            return await MapAsync(c, viewerOrEmpty: Guid.Empty, ct);
        }
        private async Task SendMentionNotificationsAsync(string body, Guid actorId, long contentId, NotificationType type, CancellationToken ct)
        {
            var usernames = GetMentionsFromBody(body);

            if (!usernames.Any()) return;

            var mentionedUserIds = await _users.GetUserIdsByUsernamesAsync(usernames, ct);

            foreach (var mentionedId in mentionedUserIds)
            {
                if (mentionedId == actorId) continue;

                // NotificationType.MentionInComment (6) kullanılıyor
                var payload = JsonSerializer.Serialize(new { actorId, contentId });
                var notif = Notification.Create(mentionedId, type, payload);
                await _notifs.AddAsync(notif, ct);
            }
            // NOT: SaveChangesAsync bu helper'ı çağıran metodun sonunda yapılacaktır.
        }

        private static IReadOnlyList<string> GetMentionsFromBody(string body)
        {
            var mentions = new List<string>();
            var words = body.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                if (word.StartsWith('@') && word.Length > 1)
                {
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
