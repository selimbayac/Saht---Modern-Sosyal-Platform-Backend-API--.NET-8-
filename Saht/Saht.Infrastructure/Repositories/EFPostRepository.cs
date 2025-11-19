using Microsoft.EntityFrameworkCore;
using Saht.Application.Abstractions;
using Saht.Domain.Posts;
using Saht.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Infrastructure.Repositories
{
    public sealed class EFPostRepository : IPostRepository
    {
        private readonly SahtDbContext _db;
        public EFPostRepository(SahtDbContext db) => _db = db;

        public Task AddAsync(Post post, CancellationToken ct = default) => _db.Posts.AddAsync(post, ct).AsTask();
        public Task<Post?> GetByIdAsync(long id, CancellationToken ct = default) =>
      _db.Posts.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

        public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
        //public IQueryable<Post> GetQueryable() => _db.Posts.AsNoTracking();
        public async Task<(List<Post> list, int totalCount)> GetFeedAsync(Guid userId, int skip, int take, CancellationToken ct = default)
        {
            var followeeIds = _db.Follows
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FolloweeId);

            var query = _db.Posts
                .Where(p => !p.IsDeleted && (p.AuthorId == userId || followeeIds.Contains(p.AuthorId)))
                .OrderByDescending(p => p.CreatedAt);

            // 1. Toplam kayıt sayısını al
            var totalCount = await query.CountAsync(ct);

            // 2. Sayfalanmış listeyi al
            var list = await query
                .Skip(skip).Take(take)
                .AsNoTracking()
                .ToListAsync(ct);

            return (list, totalCount);
        }

        public async Task<(List<Post> list, int totalCount)> GetUserPostsAsync(Guid userId, int skip, int take, CancellationToken ct = default)
        {
            var query = _db.Posts
                .Where(p => !p.IsDeleted && p.AuthorId == userId)
                .OrderByDescending(p => p.CreatedAt);

            // 1. Toplam kayıt sayısını al
            var totalCount = await query.CountAsync(ct);

            // 2. Sayfalanmış listeyi al
            var list = await query
                .Skip(skip).Take(take)
                .AsNoTracking()
                .ToListAsync(ct);

            return (list, totalCount);
        }

        public Task<bool> IsOwnerAsync(long postId, Guid userId, CancellationToken ct = default) =>
            _db.Posts.AnyAsync(p => p.Id == postId && p.AuthorId == userId, ct);

        public async Task SoftDeleteAsync(long postId, CancellationToken ct = default)
        {
            var p = await _db.Posts.FirstOrDefaultAsync(x => x.Id == postId, ct);
            if (p is null) return;
            p.SoftDelete();
            // tracked; SaveChanges çağrısında yazılacak
        }
        public async Task<(List<Post> list, int totalCount)> GetPublicPostsAsync(
    Guid viewerId,
    IReadOnlyList<Guid> blockedIds,
    int skip,
    int take,
    CancellationToken ct = default)
        {
            var query = _db.Posts.AsNoTracking()
             // Ana gönderiler ve silinmemiş olanlar
             .Where(p => p.IsDeleted == false && p.ParentPostId == null)
                // 🚨 Engellenenler listesinde olmayanların gönderileri
                .Where(p => !blockedIds.Contains(p.AuthorId))
                .OrderByDescending(p => p.CreatedAt);

            // 1. Toplam kayıt sayısını al
            var totalCount = await query.CountAsync(ct);

            // 2. Sayfalanmış listeyi al
            var list = await query
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);

            return (list, totalCount);
        }
    }
}