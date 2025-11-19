using Microsoft.EntityFrameworkCore;
using Saht.Application.Abstractions;
using Saht.Domain.Comments;
using Saht.Domain.Common;
using Saht.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Infrastructure.Repositories
{
    public sealed class EFCommentRepository : ICommentRepository
    {
        private readonly SahtDbContext _db;
        public EFCommentRepository(SahtDbContext db) => _db = db;

        public Task AddAsync(Comment c, CancellationToken ct = default)
            => _db.Comments.AddAsync(c, ct).AsTask();

        public Task<Comment?> GetByIdAsync(long id, CancellationToken ct = default)
            => _db.Comments.FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<IReadOnlyList<Comment>> ListForTargetAsync(ContentType targetType, long targetId, int skip, int take, CancellationToken ct = default)
        {
            return await _db.Comments
                .Where(x => x.TargetType == targetType && x.TargetId == targetId)
                .OrderBy(x => x.CreatedAt) // kronolojik
                .Skip(skip).Take(take)
                .ToListAsync(ct);
        }

        public Task<int> CountForTargetAsync(ContentType targetType, long targetId, CancellationToken ct = default)
            => _db.Comments.CountAsync(x => x.TargetType == targetType && x.TargetId == targetId, ct);

        public Task<bool> IsOwnerAsync(long commentId, Guid userId, CancellationToken ct = default)
            => _db.Comments.AnyAsync(x => x.Id == commentId && x.UserId == userId, ct);

        public async Task SoftDeleteAsync(long id, CancellationToken ct = default)
        {
            var c = await _db.Comments.FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new KeyNotFoundException("Yorum yok");
            c.SoftDelete();
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
