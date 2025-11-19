using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Infrastructure.Common
{
    public static class QueryableExtensions
    {
        public static async Task<(IReadOnlyList<T> list, int totalCount)> ToPagedListAsync<T>(
            this IQueryable<T> source,
            int page,
            int size,
            CancellationToken ct = default) where T : class
        {
            // Sayfa ve boyutun geçerli olduğundan emin olun
            if (page <= 0) page = 1;
            if (size <= 0 || size > 50) size = 20;

            var skip = (page - 1) * size;

            // 1. Toplam eleman sayısını al
            var totalCount = await source.CountAsync(ct);

            // 2. Sayfalanmış listeyi al
            var list = await source
                .Skip(skip)
                .Take(size)
                .ToListAsync(ct);

            return (list, totalCount);
        }
    }
}