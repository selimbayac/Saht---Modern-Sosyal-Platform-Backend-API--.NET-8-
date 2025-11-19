using Saht.Domain.Common;
using Saht.Domain.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Reports
{
    public interface IReportService
    {
        Task<Guid> CreateAsync(Guid reporterId, ContentType targetType, long targetId, string reason, CancellationToken ct = default);

        // Mod operasyonları
        Task ReviewAsync(Guid moderatorId, Guid reportId, string? note, CancellationToken ct = default);
        Task RejectAsync(Guid moderatorId, Guid reportId, string? note, CancellationToken ct = default);
        Task TakeActionAsync(Guid moderatorId, Guid reportId, string? note, CancellationToken ct = default);

        // List’ler
        Task<(IReadOnlyList<Report> Items, int Total)> ListAsync(ReportStatus? status, ContentType? type, int page, int size, CancellationToken ct = default);
        Task<(IReadOnlyList<Report> Items, int Total)> MyReportsAsync(Guid reporterId, int page, int size, CancellationToken ct = default);
        Task<(IReadOnlyList<Report> Items, int Total)> TargetReportsAsync(ContentType type, long targetId, int page, int size, CancellationToken ct = default);
    }
}
