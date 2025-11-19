using Saht.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Domain.Reports
{
    public sealed class Report : AggregateRoot<Guid>
    {
        private Report() { }

        public ContentType TargetType { get; private set; } // Post | BlogPost | ileride: User
        public long TargetId { get; private set; }          // Hedefin Id'si (User için long değilse ayrı tip ekleriz)
        public Guid ReporterId { get; private set; }        // Kim şikayet etti
        public string Reason { get; private set; } = null!; // Serbest metin (kısa)
        public ReportStatus Status { get; private set; } = ReportStatus.Pending;

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; private set; }
        public string? ModeratorNote { get; private set; }

        public static Report Create(ContentType type, long targetId, Guid reporterId, string reason)
        {
            if (targetId <= 0) throw new ArgumentOutOfRangeException(nameof(targetId));
            if (reporterId == Guid.Empty) throw new ArgumentException(nameof(reporterId));
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException(nameof(reason));

            return new Report
            {
                Id = Guid.NewGuid(),
                TargetType = type,
                TargetId = targetId,
                ReporterId = reporterId,
                Reason = reason.Trim()
            };
        }

        public void Review(string? note = null)
        {
            Status = ReportStatus.Reviewed;
            ModeratorNote = note;
            ResolvedAt = DateTime.UtcNow;
        }

        public void Reject(string? note = null)
        {
            Status = ReportStatus.Rejected;
            ModeratorNote = note;
            ResolvedAt = DateTime.UtcNow;
        }

        public void TakeAction(string? note = null)
        {
            Status = ReportStatus.ActionTaken;
            ModeratorNote = note;
            ResolvedAt = DateTime.UtcNow;
        }
    }
}
