using Saht.Domain.Common;
using Saht.Domain.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Abstractions
{
    public interface IReportRepository
    {
        Task AddAsync(Report r, CancellationToken ct = default);
        Task<Report?> GetByIdAsync(Guid id, CancellationToken ct = default);

        // Mod ekranları
        Task<(IReadOnlyList<Report> Items, int Total)> ListAsync(
            ReportStatus? status, ContentType? targetType, int skip, int take, CancellationToken ct = default);

        // Kullanıcının verdiği raporlar
        Task<(IReadOnlyList<Report> Items, int Total)> ListByReporterAsync(
            Guid reporterId, int skip, int take, CancellationToken ct = default);

        // Bir hedefe gelen raporlar (opsiyonel, mod için)
        Task<(IReadOnlyList<Report> Items, int Total)> ListByTargetAsync(
            ContentType type, long targetId, int skip, int take, CancellationToken ct = default);

        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
