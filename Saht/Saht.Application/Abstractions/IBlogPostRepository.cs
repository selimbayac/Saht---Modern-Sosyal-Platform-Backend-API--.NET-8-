using Saht.Domain.Blogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Abstractions
{
    public interface IBlogPostRepository
    {
        Task AddAsync(BlogPost b, CancellationToken ct = default);
        Task<BlogPost?> GetByIdAsync(long id, CancellationToken ct = default);
        Task<bool> IsOwnerAsync(long blogId, Guid userId, CancellationToken ct = default);
        Task SoftDeleteAsync(long blogId, CancellationToken ct = default);

        Task<IReadOnlyList<BlogPost>> ListByUserAsync(Guid userId, int skip, int take, CancellationToken ct = default);
        Task<IReadOnlyList<BlogPost>> ListRecentAsync(int skip, int take, CancellationToken ct = default);

        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
