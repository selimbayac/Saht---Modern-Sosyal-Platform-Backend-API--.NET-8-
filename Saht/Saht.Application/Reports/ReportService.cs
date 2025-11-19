using Saht.Application.Abstractions;
using Saht.Application.SocialConnections;
using Saht.Domain.Common;
using Saht.Domain.Notifications;
using Saht.Domain.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Saht.Application.Reports
{
    public sealed class ReportService : IReportService
    {
        private readonly IReportRepository _reports;
        private readonly IPostRepository _posts;
        private readonly IBlogPostRepository _blogs;
        private readonly INotificationRepository _notifs;
        private readonly IPrivacyService _privacy; // block check
        private readonly IUserRepository _users;

        public ReportService(
            IReportRepository reports,
            IPostRepository posts,
            IBlogPostRepository blogs,
            INotificationRepository notifs,
            IPrivacyService privacy,
            IUserRepository users)
        {
            _reports = reports; _posts = posts; _blogs = blogs; _notifs = notifs; _privacy = privacy; _users = users;
        }

        public async Task<Guid> CreateAsync(Guid reporterId, ContentType targetType, long targetId, string reason, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Sebep zorunlu");

            // Hedef doğrulama + ownerId çek
            Guid ownerId = targetType switch
            {
                ContentType.Post =>
                    (await _posts.GetByIdAsync(targetId, ct))?.AuthorId
                    ?? throw new KeyNotFoundException("Hedef (Post) bulunamadı"),

                ContentType.BlogPost =>
                    (await _blogs.GetByIdAsync(targetId, ct))?.AuthorId
                    ?? throw new KeyNotFoundException("Hedef (BlogPost) bulunamadı"),

                _ => throw new NotSupportedException("Bu targetType desteklenmiyor")
            };

            // Raporu yaz
            var r = Report.Create(targetType, targetId, reporterId, reason);
            await _reports.AddAsync(r, ct);
            await _reports.SaveChangesAsync(ct);

            // İSTERSEN: burada mod’a bildirim atmayı ŞİMDİLİK atlıyoruz.
            // Moderasyon paneli zaten Reports tablosundan listeler.

            return r.Id;
        }


        public async Task ReviewAsync(Guid modId, Guid reportId, string? note, CancellationToken ct = default)
        {
            var r = await _reports.GetByIdAsync(reportId, ct) ?? throw new KeyNotFoundException();
            r.Review(note);
            await _reports.SaveChangesAsync(ct);
            await NotifyDecisionAsync(r, "review", ct);
        }

        public async Task RejectAsync(Guid modId, Guid reportId, string? note, CancellationToken ct = default)
        {
            var r = await _reports.GetByIdAsync(reportId, ct) ?? throw new KeyNotFoundException();
            r.Reject(note);
            await _reports.SaveChangesAsync(ct);
            await NotifyDecisionAsync(r, "reject", ct);
        }

        public async Task TakeActionAsync(Guid modId, Guid reportId, string? note, CancellationToken ct = default)
        {
            var r = await _reports.GetByIdAsync(reportId, ct) ?? throw new KeyNotFoundException();
            r.TakeAction(note);
            await _reports.SaveChangesAsync(ct);
            await NotifyDecisionAsync(r, "action", ct);
        }

        private async Task NotifyDecisionAsync(Report r, string decision, CancellationToken ct)
        {
            var payload = JsonSerializer.Serialize(new
            {
                reportId = r.Id,
                decision,
                targetType = (int)r.TargetType,
                targetId = r.TargetId
            });
            await _notifs.AddAsync(Notification.Create(r.ReporterId, NotificationType.ReportDecision, payload), ct);
            await _notifs.SaveChangesAsync(ct);
        }

        public async Task<(IReadOnlyList<Report> Items, int Total)> ListAsync(ReportStatus? status, ContentType? type, int page, int size, CancellationToken ct = default)
        {
            (page, size) = Normalize(page, size);
            return await _reports.ListAsync(status, type, (page - 1) * size, size, ct);
        }

        public async Task<(IReadOnlyList<Report> Items, int Total)> MyReportsAsync(Guid reporterId, int page, int size, CancellationToken ct = default)
        {
            (page, size) = Normalize(page, size);
            return await _reports.ListByReporterAsync(reporterId, (page - 1) * size, size, ct);
        }

        public async Task<(IReadOnlyList<Report> Items, int Total)> TargetReportsAsync(ContentType type, long targetId, int page, int size, CancellationToken ct = default)
        {
            (page, size) = Normalize(page, size);
            return await _reports.ListByTargetAsync(type, targetId, (page - 1) * size, size, ct);
        }

        private static (int page, int size) Normalize(int page, int size)
        {
            page = Math.Max(1, page);
            size = Math.Clamp(size, 1, 50);
            return (page, size);
        }
    }
}
