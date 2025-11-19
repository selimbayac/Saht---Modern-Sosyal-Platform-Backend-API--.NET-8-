using Saht.Domain.Posts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Abstractions
{
    public interface IPostRepository
    {
        Task AddAsync(Post post, CancellationToken ct = default);
        Task<Post?> GetByIdAsync(long id, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);

        // Feeds & timelines
        
        Task<(List<Post> list, int totalCount)> GetFeedAsync(Guid userId, int skip, int take, CancellationToken ct = default);
        Task<(List<Post> list, int totalCount)> GetUserPostsAsync(Guid userId, int skip, int take, CancellationToken ct = default);
        // IQueryable tabanlı filtreleme
       // IQueryable<Post> GetQueryable();
        Task<bool> IsOwnerAsync(long postId, Guid userId, CancellationToken ct = default);
        Task SoftDeleteAsync(long postId, CancellationToken ct = default);

        Task<(List<Post> list, int totalCount)> GetPublicPostsAsync(
             Guid viewerId, // Engellenenleri filtrelemek için
             IReadOnlyList<Guid> blockedIds, // Uygulanacak Engellenen ID'ler listesi
             int skip,
             int take,
             CancellationToken ct = default);
    }
}
