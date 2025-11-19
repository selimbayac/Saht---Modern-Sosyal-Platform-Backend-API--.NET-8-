using Microsoft.EntityFrameworkCore;
using Saht.Application.Abstractions;
using Saht.Domain.Common;
using Saht.Domain.Reports;
using Saht.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Infrastructure.Repositories
{
    public sealed class EFReportRepository : IReportRepository
    {
        private readonly SahtDbContext _db;
        public EFReportRepository(SahtDbContext db) => _db = db;

        public Task AddAsync(Report r, CancellationToken ct = default)
            => _db.Reports.AddAsync(r, ct).AsTask();

        public Task<Report?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _db.Reports.FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<(IReadOnlyList<Report> Items, int Total)> ListAsync(
            ReportStatus? status, ContentType? targetType, int skip, int take, CancellationToken ct = default)
        {
            var q = _db.Reports.AsQueryable();
            if (status.HasValue) q = q.Where(x => x.Status == status);
            if (targetType.HasValue) q = q.Where(x => x.TargetType == targetType);
            var total = await q.CountAsync(ct);
            var items = await q.OrderByDescending(x => x.CreatedAt)
                               .Skip(skip).Take(take).ToListAsync(ct);
            return (items, total);
        }

        public async Task<(IReadOnlyList<Report> Items, int Total)> ListByReporterAsync(
            Guid reporterId, int skip, int take, CancellationToken ct = default)
        {
            var q = _db.Reports.Where(x => x.ReporterId == reporterId);
            var total = await q.CountAsync(ct);
            var items = await q.OrderByDescending(x => x.CreatedAt)
                               .Skip(skip).Take(take).ToListAsync(ct);
            return (items, total);
        }

        public async Task<(IReadOnlyList<Report> Items, int Total)> ListByTargetAsync(
            ContentType type, long targetId, int skip, int take, CancellationToken ct = default)
        {
            var q = _db.Reports.Where(x => x.TargetType == type && x.TargetId == targetId);
            var total = await q.CountAsync(ct);
            var items = await q.OrderByDescending(x => x.CreatedAt)
                               .Skip(skip).Take(take).ToListAsync(ct);
            return (items, total);
        }

        public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}
