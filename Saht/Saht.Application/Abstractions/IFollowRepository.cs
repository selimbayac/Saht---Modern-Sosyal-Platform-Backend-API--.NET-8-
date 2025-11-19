using Saht.Domain.Follows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Abstractions
{
    public interface IFollowRepository
    {
        Task AddAsync(Follow f, CancellationToken ct = default);
        Task RemoveAsync(Guid followerId, Guid followeeId, CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid followerId, Guid followeeId, CancellationToken ct = default);

        Task<int> CountFollowersAsync(Guid userId, CancellationToken ct = default);
        Task<int> CountFollowingAsync(Guid userId, CancellationToken ct = default);

        Task<IReadOnlyList<Guid>> GetFollowingIdsAsync(Guid userId, int skip, int take, CancellationToken ct = default);
        Task<IReadOnlyList<Guid>> GetFollowerIdsAsync(Guid userId, int skip, int take, CancellationToken ct = default);
        Task<IReadOnlyList<Guid>> GetFollowerIdsOfAsync(Guid userId, CancellationToken ct = default);// takipci şeyi
        Task SaveChangesAsync(CancellationToken ct = default);

     
      
    }
}
