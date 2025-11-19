using Microsoft.EntityFrameworkCore;
using Saht.Application.Abstractions;
using Saht.Domain.Blogs;
using Saht.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Infrastructure.Repositories
{
    public sealed class EFBlogPostRepository : IBlogPostRepository
    {
        private readonly SahtDbContext _db;
        public EFBlogPostRepository(SahtDbContext db) { _db = db; }

        public Task<BlogPost?> GetByIdAsync(long id, CancellationToken ct = default)
            => _db.BlogPosts.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        public Task<bool> IsOwnerAsync(long blogId, Guid userId, CancellationToken ct = default)
            => _db.BlogPosts.AnyAsync(x => x.Id == blogId && x.AuthorId == userId && !x.IsDeleted, ct);

        public async Task AddAsync(BlogPost b, CancellationToken ct = default) => await _db.BlogPosts.AddAsync(b, ct);

        public Task SoftDeleteAsync(long blogId, CancellationToken ct = default)
            => _db.BlogPosts.Where(x => x.Id == blogId).ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDeleted, true), ct);

        public Task<IReadOnlyList<BlogPost>> ListByUserAsync(Guid userId, int skip, int take, CancellationToken ct = default)
            => _db.BlogPosts.Where(x => x.AuthorId == userId && !x.IsDeleted)
                            .OrderByDescending(x => x.CreatedAt)
                            .Skip(skip).Take(take)
                            .ToListAsync(ct)
                            .ContinueWith(t => (IReadOnlyList<BlogPost>)t.Result, ct);

        public Task<IReadOnlyList<BlogPost>> ListRecentAsync(int skip, int take, CancellationToken ct = default)
            => _db.BlogPosts.Where(x => !x.IsDeleted)
                            .OrderByDescending(x => x.CreatedAt)
                            .Skip(skip).Take(take)
                            .ToListAsync(ct)
                            .ContinueWith(t => (IReadOnlyList<BlogPost>)t.Result, ct);

        public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}
