using Saht.Domain.Comments;
using Saht.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Abstractions
{
    public interface ICommentRepository
    {
        Task AddAsync(Comment c, CancellationToken ct = default);
        Task<Comment?> GetByIdAsync(long id, CancellationToken ct = default);

        Task<IReadOnlyList<Comment>> ListForTargetAsync(
            ContentType targetType, long targetId, int skip, int take, CancellationToken ct = default);

        Task<int> CountForTargetAsync(ContentType targetType, long targetId, CancellationToken ct = default);

        Task<bool> IsOwnerAsync(long commentId, Guid userId, CancellationToken ct = default);

        Task SoftDeleteAsync(long id, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
